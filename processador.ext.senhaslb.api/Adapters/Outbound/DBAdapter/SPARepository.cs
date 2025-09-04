using Adapters.Outbound.DBAdapter.Model;
using Adapters.Outbound.OtlpAdapter;
using Microsoft.Extensions.Options;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using Domain.Core.Models.SPA;
using System.Data.SqlClient;
using System.Globalization;
using System.Diagnostics;
using Domain.Core.Enums;
using Domain.Core.Base;
using System.Data;
using System.Text;
using Dapper;

namespace Adapters.Outbound.DBAdapter
{
    public class SPARepository : BaseService, ISPARepository
    {
        #region variáveis

        private readonly IDBAdapterConnection _dbConnection;
        private readonly int _commandTimeout;
        private IDbConnection? _session;
        private readonly IOptions<DBSettings> _dbsettings;
        internal StringBuilder? _stringBuilder = new StringBuilder();

        #endregion

        public SPARepository(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<SPARepository>>();
            _dbConnection = serviceProvider.GetRequiredService<IDBAdapterConnection>();
            _dbsettings = serviceProvider.GetRequiredService<IOptions<DBSettings>>();
            _commandTimeout = 30;
        } 

        public async ValueTask IniciarSPATransacao(int agencia, int posto, SPATransacao spaTransacao)
        {
            try
            {
                await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
                {
                    var _parameters = new DynamicParameters();
                    int _gravalog = 0;

                    //INPUT
                    _parameters.Add("@psmlAgencia", agencia);
                    _parameters.Add("@ptinPosto", posto);
                    _parameters.Add("@pintTransacao", spaTransacao.Codigo);

                    //OUTPUT
                    _parameters.Add("@pvchDescricao", spaTransacao.Descricao, DbType.String, ParameterDirection.Output, size: int.MaxValue);
                    _parameters.Add("@pdatContabil", spaTransacao.DataContabil, DbType.DateTime, ParameterDirection.Output);
                    _parameters.Add("@pdatSQLConfig", spaTransacao.FormatoDatas, DbType.DateTime, ParameterDirection.Output);
                    _parameters.Add("@pnumSQLConfig", spaTransacao.FormatoValores, DbType.Decimal, ParameterDirection.Output);
                    _parameters.Add("@pvchProcedureSQL", spaTransacao.ProcedureSQL, DbType.String, ParameterDirection.Output, size: int.MaxValue);
                    _parameters.Add("@psmlTimeOut", spaTransacao.Timeout, DbType.Int32, ParameterDirection.Output);
                    _parameters.Add("@ptinLOG", _gravalog, DbType.Int32, ParameterDirection.Output);
                    _parameters.Add("@psmlNumeroParametros", spaTransacao.NumeroParametros, DbType.Int32, ParameterDirection.Output);

                    await _connection.ExecuteAsync("spx_IniciarTransacao", _parameters,
                            commandTimeout: _commandTimeout,
                            commandType: CommandType.StoredProcedure);
                 
                    //OUTPUT
                    spaTransacao.Descricao = _parameters.Get<string>("@pvchDescricao");
                    spaTransacao.DataContabil = _parameters.Get<DateTime>("@pdatContabil");
                    spaTransacao.ProcedureSQL = _parameters.Get<string>("@pvchProcedureSQL");
                    spaTransacao.NumeroParametros = _parameters.Get<int>("@psmlNumeroParametros");
                    spaTransacao.FormatoValores = _parameters.Get<string>("@pnumSQLConfig");
                    spaTransacao.Timeout = _parameters.Get<int>("@psmlTimeOut");
                    spaTransacao.TransacaoGravaLog = _parameters.Get<int>("@ptinLOG") == 7 ? true : false;

                    CarregarSPAParametros(ref spaTransacao);

                    return true;
                });
            }
            catch (Exception ex)
            {
                throw handleError(ex, "IniciarSPATransacao");
            }
        }

        public async ValueTask<BaseReturn> ExecutaDB(SPATransacao spaTransacao)
        {
            using var _activity = OtlpActivityService.GenerateActivitySource.StartActivity($"### EXECUTANDO TRANSAÇÃO {spaTransacao.Codigo} " +
                                                                                           $"NO BANCO DE DADOS - AÇÃO {((EnumAcao)spaTransacao.ParametrosFixos!.Acao)} ####", 
                                                                                           ActivityKind.Internal);

            try
            {
                return await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
                {
                    var cmd = CriarSqlCommand(spaTransacao, _connection);
                    AdicionarParametrosAoComando(spaTransacao, cmd);

                    if (_dbsettings.Value.IsTraceExecActive)
                        RegistrarExecucaoSQL(spaTransacao, cmd, _activity, "ExecuteSQL");

                    await ExecutarComandoComTracing(spaTransacao, cmd, _activity);

                    GravarParametrosSaida(spaTransacao, cmd);

                    if (_dbsettings.Value.IsTraceExecActive)
                        RegistrarExecucaoSQL(spaTransacao, cmd, _activity, "RetornoSQL");

                    if (spaTransacao.ParametrosFixos.Acao != (int)EnumAcao.ACAO_VALIDAR)
                        await GravarParametroLog(spaTransacao);

                    return new BaseReturn();
                });
            }
            catch (Exception ex)
            {
                TratarErro(_activity, ex);
                throw handleError(ex, "ProcessadorSPA: SPARepository: ExecutaDB");
            }
        }

        private SqlCommand CriarSqlCommand(SPATransacao spaTransacao, IDbConnection connection)
        {
            return new SqlCommand
            {
                Connection = (SqlConnection)connection,
                CommandTimeout = spaTransacao.Timeout,
                CommandText = spaTransacao.ProcedureSQL,
                CommandType = CommandType.StoredProcedure
            };
        }

        private static void AdicionarParametrosAoComando(SPATransacao spaTransacao, SqlCommand cmd)
        {
            foreach (var item in spaTransacao.ListParametros!)
            {
                if (item.Direcao == ParameterDirection.Output)
                {
                    cmd.Parameters.Add(item.RecuperarSqlParameterOutput());
                }
                else if (item.Direcao == ParameterDirection.Input || item.Direcao == ParameterDirection.InputOutput)
                {
                    var valor = Convert.ToString(item._oSQLParameter!.Value);
                    if (item.Indice <= 24 || !string.IsNullOrEmpty(valor))
                    {
                        cmd.Parameters.Add(item.RecuperarSqlParameterInput());
                    }
                    else
                    {
                        cmd.Parameters.Add(item.RecuperarSqlParameterOutput());
                    }
                }
            }
        }

        private async Task ExecutarComandoComTracing(SPATransacao spaTransacao, SqlCommand cmd, Activity? activity)
        {
            var trace = SPExec(ref spaTransacao, ref cmd);
            activity?.SetTag("ExecuteSQL", trace);

            await cmd.ExecuteNonQueryAsync();
        }

        private static void GravarParametrosSaida(SPATransacao spaTransacao, SqlCommand cmd)
        {
            foreach (SqlParameter param in cmd.Parameters)
            {
                var paramItem = spaTransacao.ListParametros!.Find(item => item.Nome == param.ParameterName);
                if (paramItem != null)
                {
                    paramItem._oSQLParameter!.Value = param.Value;
                }
            }
        }

        private void RegistrarExecucaoSQL(SPATransacao spaTransacao, SqlCommand cmd, Activity? activity, string tag)
        {
            var trace = SPExec(ref spaTransacao, ref cmd);
            activity?.SetTag(tag, trace);
        }

        private void TratarErro(Activity? activity, Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.SetTag("Erro", ex.Message);
            activity?.SetTag("Stacktrace", ex.StackTrace ?? string.Empty);
        }

        public async ValueTask<EnumSPASituacaoTransacao> RecuperarSituacao(SPATransacao spaTransacao)
        {
            try
            {
                return await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
                {
                    var _parameters = new DynamicParameters();

                    var parsedDate = DateTime.ParseExact(
                         spaTransacao.ParametrosFixos!.DataContabil!,
                         "dd/MM/yyyy HH:mm:ss",
                         CultureInfo.InvariantCulture);

                    //INPUT
                    _parameters.Add("@psmlAgencia", spaTransacao.ParametrosFixos.Agencia1);
                    _parameters.Add("@ptinPosto", spaTransacao.ParametrosFixos.Posto1);
                    _parameters.Add("@pdatContabil", parsedDate, DbType.DateTime);
                    _parameters.Add("@pintNSU", spaTransacao.ParametrosFixos.NSU1);
                    _parameters.Add("@pchrInterag", "R");

                    //OUTPUT          
                    _parameters.Add("@ptinSituacao", spaTransacao.Descricao, DbType.Int32, ParameterDirection.Output, size: int.MaxValue);

                    await _connection.ExecuteAsync("spx_RecuperarSituacao", _parameters,
                        commandTimeout: _commandTimeout,
                        commandType: CommandType.StoredProcedure);

                    //OUTPUT
                    var _situacao = (EnumSPASituacaoTransacao)_parameters.Get<int>("@ptinSituacao");

                    return _situacao;
                });
            }
            catch (Exception ex)
            {
                throw handleError(ex, "RecuperarSituacao");
            }
        }

        public async ValueTask GravarParametroLog(SPATransacao spaTransacao)
        {
            try
            {
                await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
                {
                    var _parameters = new DynamicParameters();

                    var parsedDate = DateTime.ParseExact(
                          spaTransacao.ParametrosFixos!.DataContabil!,
                          "dd/MM/yyyy HH:mm:ss",
                          CultureInfo.InvariantCulture);

                    //INPUT
                    _parameters.Add("@psmlBanco", 37);
                    _parameters.Add("@psmlAgencia", spaTransacao.ParametrosFixos.Agencia1);
                    _parameters.Add("@ptinPosto", spaTransacao.ParametrosFixos.Posto1);
                    _parameters.Add("@pdatContabil", parsedDate, DbType.DateTime);
                    _parameters.Add("@pintNSU", spaTransacao.ParametrosFixos.NSU1);
                    _parameters.Add("@pchrInterag", spaTransacao.ParametrosFixos.TipoTransacao);

                    //OUTPUT
                    for (int i = 1; i < 6; i++)
                    {
                        _parameters.Add($"@pvchParam{i}", spaTransacao.ParamsToLog().ElementAtOrDefault(i - 1) ?? "", DbType.String, ParameterDirection.Input);
                    }

                    await _connection.ExecuteAsync("spx_ParametroLog", _parameters,
                        commandTimeout: _commandTimeout,
                        commandType: CommandType.StoredProcedure);
          
                    return true;
                });
            }
            catch (Exception ex)
            {
                LogError($"[GravarParametroLog]", ex);
            }
        }

        public virtual void CarregarSPAParametros(ref SPATransacao spaTransacao)
        {
            _session = _dbConnection.GetConnectionAsync().Result;

            SqlCommand cmd = new SqlCommand(spaTransacao.ProcedureSQL, (SqlConnection)_session);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = _commandTimeout;
            SqlCommandBuilder.DeriveParameters(cmd);

            int i = 0;
            spaTransacao.ListParametros!.Clear();
            cmd.Parameters.RemoveAt(0);

            foreach (SqlParameter p in cmd.Parameters)
            {
                spaTransacao.ListParametros.Add(new SPAParametro(p, i++));
            }
        }

        internal string SPExec(ref SPATransacao transacao, ref SqlCommand cmd)
        {
            _stringBuilder = new StringBuilder("EXECUTE ");
            _stringBuilder.Append(transacao.ProcedureSQL);
            _stringBuilder.AppendLine();

            foreach (SqlParameter param in cmd.Parameters)
            {
                _stringBuilder.Append("\r\n");
                _stringBuilder.Append(param.ParameterName);
                _stringBuilder.Append("= ");

                if (param.SqlDbType == SqlDbType.Char || param.SqlDbType == SqlDbType.VarChar || param.SqlDbType == SqlDbType.DateTime || param.SqlDbType == SqlDbType.SmallDateTime)
                {
                    _stringBuilder.Append($"'{param.Value}'");
                    continue;
                }

                _stringBuilder.Append(param.Value);
            }

            return _stringBuilder.ToString();
        }

        public async Task<IEnumerable<RetornoSPX>> ExecutarSPXIdentificaCartao(ParametrosSPX parametros)
        {

            var parameters = new DynamicParameters();
            parameters.Add("@pvchDados", parametros.Dados, DbType.String);
            parameters.Add("@pvchResposta", parametros.Resposta, DbType.String);
            parameters.Add("@pvchSequencia1", parametros.Seq1, DbType.String);
            parameters.Add("@pvchSequencia2", parametros.Seq2, DbType.String);
            parameters.Add("@pvchSequencia3", parametros.Seq3 , DbType.String);
            parameters.Add("@pvchGrupo", parametros.Grupo, DbType.String);
            parameters.Add("@pintToken", parametros.Token, DbType.Int32);

            var _result = await _session!.QueryAsync<RetornoSPX>("dbo.spx_IdentificaCartao2", parameters, commandType: CommandType.StoredProcedure);
            return _result;
        }

        public async Task<IEnumerable<RetornoSPX>> ExecutarSPXSenhaSilabica(ParametrosSPX parametros)
        {

            var parameters = new DynamicParameters();
            parameters.Add("@pvchDados", parametros.Dados, DbType.String);
            parameters.Add("@", parametros.PvchMsgIN, DbType.String);
            parameters.Add("@", parametros.PvchMsgOUT, DbType.String);
            parameters.Add("@", parametros.PIntRetDLL, DbType.Int32);

            var _result = await _session!.QueryAsync<RetornoSPX>("dbo.spx_SenhaSilabica2", parameters, commandType: CommandType.StoredProcedure);
            return _result;
        }

        ~SPARepository()
        {
            _stringBuilder = null;
        }
    }
}
