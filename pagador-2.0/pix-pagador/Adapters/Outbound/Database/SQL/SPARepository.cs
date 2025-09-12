using Dapper;
using Domain.Core.Common.Base;
using Domain.Core.Constant;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using System.Data;

namespace Adapters.Outbound.Database.SQL
{
    public class SPARepository : BaseSQLRepository, ISPARepository
    {

        public SPARepository(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        public async ValueTask<string> EfetivarOrdemPagamento(TransactionEfetivarOrdemPagamento transaction)
        {
            using var operationContext = _loggingAdapter.StartOperation("EfetivarPagamento", transaction.CorrelationId);
            string _mensagemPixOut = string.Empty;

            _loggingAdapter.AddProperty("Chave Idempotencia", transaction.chaveIdempotencia);

            await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
            {

                var _parameters = new DynamicParameters();
                _parameters.Add("@pvchOperador", OperationConstants.DEFAULT_OPERADOR);
                _parameters.Add("@ptinCanal", byte.Parse(transaction.canal.ToString()));
                _parameters.Add("@psmlAgencia", OperationConstants.DEFAULT_AGENCIA);
                _parameters.Add("@ptinPosto", OperationConstants.DEFAULT_POSTO);
                _parameters.Add("@pvchEstacao", OperationConstants.DEFAULT_ESTACAO);
                _parameters.Add("@pvchChvIdemPotencia", transaction.chaveIdempotencia);
                _parameters.Add("@pvchMsgPixIN", transaction.getTransactionSerialization());

                // Parâmetro de saída
                _parameters.Add("@pvchMsgPixOUT", dbType: DbType.String, direction: ParameterDirection.InputOutput, size: 4000);

                await _connection.ExecuteAsync("sps_PixDebito", _parameters,
                        commandTimeout: _dbsettings.Value.CommandTimeout,
                        commandType: CommandType.StoredProcedure);

                _mensagemPixOut = _parameters.Get<string>("@pvchMsgPixOUT");
            });

            return _mensagemPixOut;

        }


        public async ValueTask<string> RegistrarOrdemPagamento(TransactionRegistrarOrdemPagamento transaction)
        {

            using var operationContext = _loggingAdapter.StartOperation("IniciarPagamento", transaction.CorrelationId);
            string _mensagemPixOut = string.Empty;

            _loggingAdapter.AddProperty("Pagador Agencia", transaction.pagador.nrAgencia);
            _loggingAdapter.AddProperty("Pagador Conta", transaction.pagador.nrConta);
            _loggingAdapter.AddProperty("Pagador CPF-CNPJ", transaction.pagador.cpfCnpj.ToString());

            _loggingAdapter.AddProperty("Recebedor Agencia", transaction.recebedor.nrAgencia);
            _loggingAdapter.AddProperty("Recebedor Conta", transaction.recebedor.nrConta);
            _loggingAdapter.AddProperty("Recebedor CPF-CNPJ", transaction.recebedor.cpfCnpj.ToString());
            _loggingAdapter.AddProperty("Chave Idempotencia", transaction.chaveIdempotencia);
            _loggingAdapter.AddProperty("Valor", transaction.valor.ToString());

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


                await _connection.ExecuteAsync("sps_PixBloqueioSaldo", _parameters,
                        commandTimeout: _dbsettings.Value.CommandTimeout,
                        commandType: CommandType.StoredProcedure);

                _mensagemPixOut = _parameters.Get<string>("@pvchMsgPixOUT");
            });

            return _mensagemPixOut;
        }


        public async ValueTask<string> CancelarOrdemPagamento(TransactionCancelarOrdemPagamento transaction)
        {

            using var operationContext = _loggingAdapter.StartOperation("CancelarPagamento", transaction.CorrelationId);
            string _mensagemPixOut = string.Empty;

            _loggingAdapter.AddProperty("Chave Idempotencia", transaction.chaveIdempotencia);

            await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
            {

                var _parameters = new DynamicParameters();
                _parameters.Add("@pvchOperador", OperationConstants.DEFAULT_OPERADOR);
                _parameters.Add("@ptinCanal", byte.Parse(transaction.canal.ToString()));
                _parameters.Add("@psmlAgencia", OperationConstants.DEFAULT_AGENCIA);
                _parameters.Add("@ptinPosto", OperationConstants.DEFAULT_POSTO);
                _parameters.Add("@pvchEstacao", OperationConstants.DEFAULT_ESTACAO);
                _parameters.Add("@pvchChvIdemPotencia", transaction.chaveIdempotencia);
                _parameters.Add("@pvchMsgPixIN", transaction.getTransactionSerialization());

                // Parâmetro de saída
                _parameters.Add("@pvchMsgPixOUT", dbType: DbType.String, direction: ParameterDirection.InputOutput, size: 4000);

                await _connection.ExecuteAsync("sps_PixDesbloqueioSaldo", _parameters,
                        commandTimeout: _dbsettings.Value.CommandTimeout,
                        commandType: CommandType.StoredProcedure);

                _mensagemPixOut = _parameters.Get<string>("@pvchMsgPixOUT");
            });

            return _mensagemPixOut;

        }


        public async ValueTask<string> RegistrarOrdemDevolucao(TransactionRegistrarOrdemDevolucao transaction)
        {

            using var operationContext = _loggingAdapter.StartOperation("IniciarDevolucao", transaction.CorrelationId);
            string _mensagemPixOut = string.Empty;

            _loggingAdapter.AddProperty("Chave Idempotencia", transaction.chaveIdempotencia);

            await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
            {

                var _parameters = new DynamicParameters();
                _parameters.Add("@pvchOperador", OperationConstants.DEFAULT_OPERADOR);
                _parameters.Add("@ptinCanal", byte.Parse(transaction.canal.ToString()));
                _parameters.Add("@psmlAgencia", OperationConstants.DEFAULT_AGENCIA);
                _parameters.Add("@ptinPosto", OperationConstants.DEFAULT_POSTO);
                _parameters.Add("@pvchEstacao", OperationConstants.DEFAULT_ESTACAO);
                _parameters.Add("@pvchChvIdemPotencia", transaction.chaveIdempotencia);
                _parameters.Add("@pvchMsgPixIN", transaction.getTransactionSerialization());

                // Parâmetro de saída
                _parameters.Add("@pvchMsgPixOUT", dbType: DbType.String, direction: ParameterDirection.InputOutput, size: 4000);

                await _connection.ExecuteAsync("sps_PixBloqueioSaldoDevolucao", _parameters,
                        commandTimeout: _dbsettings.Value.CommandTimeout,
                        commandType: CommandType.StoredProcedure);

                _mensagemPixOut = _parameters.Get<string>("@pvchMsgPixOUT");
            });

            return _mensagemPixOut;

        }


        public async ValueTask<string> EfetivarOrdemDevolucao(TransactionEfetivarOrdemDevolucao transaction)
        {
            using var operationContext = _loggingAdapter.StartOperation("EfetivarDevolucao", transaction.CorrelationId);
            string _mensagemPixOut = string.Empty;

            _loggingAdapter.AddProperty("Chave Idempotencia", transaction.chaveIdempotencia);

            await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
            {

                var _parameters = new DynamicParameters();
                _parameters.Add("@pvchOperador", OperationConstants.DEFAULT_OPERADOR);
                _parameters.Add("@ptinCanal", byte.Parse(transaction.canal.ToString()));
                _parameters.Add("@psmlAgencia", OperationConstants.DEFAULT_AGENCIA);
                _parameters.Add("@ptinPosto", OperationConstants.DEFAULT_POSTO);
                _parameters.Add("@pvchEstacao", OperationConstants.DEFAULT_ESTACAO);
                _parameters.Add("@pvchChvIdemPotencia", transaction.chaveIdempotencia);
                _parameters.Add("@pvchMsgPixIN", transaction.getTransactionSerialization());

                // Parâmetro de saída
                _parameters.Add("@pvchMsgPixOUT", dbType: DbType.String, direction: ParameterDirection.InputOutput, size: 4000);

                await _connection.ExecuteAsync("sps_PixDevolucaoCredito", _parameters,
                        commandTimeout: _dbsettings.Value.CommandTimeout,
                        commandType: CommandType.StoredProcedure);

                _mensagemPixOut = _parameters.Get<string>("@pvchMsgPixOUT");
            });

            return _mensagemPixOut;
        }


        public async ValueTask<string> CancelarOrdemDevolucao(TransactionCancelarOrdemDevolucao transaction)
        {
            using var operationContext = _loggingAdapter.StartOperation("CancelarDevolucao", transaction.CorrelationId);
            string _mensagemPixOut = string.Empty;

            _loggingAdapter.AddProperty("Chave Idempotencia", transaction.chaveIdempotencia);

            await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
            {

                var _parameters = new DynamicParameters();
                _parameters.Add("@pvchOperador", OperationConstants.DEFAULT_OPERADOR);
                _parameters.Add("@ptinCanal", byte.Parse(transaction.canal.ToString()));
                _parameters.Add("@psmlAgencia", OperationConstants.DEFAULT_AGENCIA);
                _parameters.Add("@ptinPosto", OperationConstants.DEFAULT_POSTO);
                _parameters.Add("@pvchEstacao", OperationConstants.DEFAULT_ESTACAO);
                _parameters.Add("@pvchChvIdemPotencia", transaction.chaveIdempotencia);
                _parameters.Add("@pvchMsgPixIN", transaction.getTransactionSerialization());

                // Parâmetro de saída
                _parameters.Add("@pvchMsgPixOUT", dbType: DbType.String, direction: ParameterDirection.InputOutput, size: 4000);

                await _connection.ExecuteAsync("sps_PixDesbloqueioSaldoDevolucao", _parameters,
                        commandTimeout: _dbsettings.Value.CommandTimeout,
                        commandType: CommandType.StoredProcedure);

                _mensagemPixOut = _parameters.Get<string>("@pvchMsgPixOUT");
            });

            return _mensagemPixOut;

        }



    }
}
