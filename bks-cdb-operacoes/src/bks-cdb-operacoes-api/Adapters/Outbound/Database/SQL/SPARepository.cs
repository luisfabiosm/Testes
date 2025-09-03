using Dapper;
using Domain.Core.Common.Base;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Constant;
using Domain.Core.Models.Transactions;
using Domain.Core.Ports.Outbound;
using Domain.Core.Settings;
using Microsoft.Extensions.Options;
using System.Data;

namespace Adapters.Outbound.Database.SQL
{
    public class SPARepository: BaseSQLRepository, ISPARepository
    {
        private readonly IOptions<CDBSettings> _cdbSettings;
        public SPARepository(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _cdbSettings = serviceProvider.GetRequiredService<IOptions<CDBSettings>>();
        }

        public async ValueTask<string> ExecuteTransaction<TResponse>(BaseTransaction<BaseReturn<TResponse>> transaction)
            where TResponse : BaseTransactionResponse
        {          
            using var operationContext = _loggingAdapter.StartOperation("ConsultaLista", transaction.CorrelationId);
            string _mensagemPixOut = string.Empty;
   
            _loggingAdapter.AddProperty("Chave Idempotencia", transaction.chaveIdempotencia);
         
            var _msgIn = transaction.getTransactionSerialization();
            _loggingAdapter.AddProperty("@pvchMsgPixIN", _msgIn);
            _loggingAdapter.LogInformation(_msgIn);

            await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
            {

                var _parameters = new DynamicParameters();
                _parameters.Add("@pvchOperador", OperationConstants.DEFAULT_OPERADOR);
                _parameters.Add("@ptinCanal", byte.Parse(transaction.canal.ToString()));
                _parameters.Add("@psmlAgencia", OperationConstants.DEFAULT_AGENCIA);
                _parameters.Add("@ptinPosto", OperationConstants.DEFAULT_POSTO);
                _parameters.Add("@pvchEstacao", OperationConstants.DEFAULT_ESTACAO);
                _parameters.Add("@pvchChvIdemPotencia", transaction.chaveIdempotencia);
                _parameters.Add("@pvchMsgPixIN", _msgIn);

                // Parâmetro de saída
                _parameters.Add("@pvchMsgPixOUT", dbType: DbType.String, direction: ParameterDirection.InputOutput, size: 4000);


                await _connection.ExecuteAsync("sps_CDBSolicitaLista", _parameters,
                        commandTimeout: _dbsettings.Value.CommandTimeout,
                        commandType: CommandType.StoredProcedure);

                _mensagemPixOut = _parameters.Get<string>("@pvchMsgPixOUT");
            });

            return _mensagemPixOut;

        }
    }
}
