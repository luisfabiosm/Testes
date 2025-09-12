using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Pagamento.EfetivarOrdemPagamento
{
    public class EfetivarOrdemPagamentoHandler : BSUseCaseHandler<TransactionEfetivarOrdemPagamento, BaseReturn<JDPIEfetivarOrdemPagamentoResponse>, JDPIEfetivarOrdemPagamentoResponse>
    {

        public EfetivarOrdemPagamentoHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }


        public override async Task<ValidationResult> ExecuteSpecificValidations(TransactionEfetivarOrdemPagamento transaction, CancellationToken cancellationToken)
        {
            var errors = new List<ErrorDetails>();

            var clienteValidation = _validateService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
            if (!clienteValidation.IsValid)
                errors.AddRange(clienteValidation.Errors);

            var endToEndValidation = _validateService.ValidarEndToEndId(transaction.endToEndId);
            if (!endToEndValidation.IsValid)
                errors.AddRange(endToEndValidation.Errors);

            return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
        }


        public override async Task<JDPIEfetivarOrdemPagamentoResponse> ExecuteTransactionProcessing(TransactionEfetivarOrdemPagamento transaction, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _spaRepoSql.EfetivarOrdemPagamento(transaction);
                return new JDPIEfetivarOrdemPagamentoResponse(result);
            }
            catch (BusinessException bex)
            {
                _loggingAdapter.LogError("Erro retornado pela Sps", bex);
                throw;
            }
            catch (Exception dbEx)
            {
                _loggingAdapter.LogError("Erro de database durante efetivacao", dbEx);
                throw;
            }
        }


        public override BaseReturn<JDPIEfetivarOrdemPagamentoResponse> ReturnSuccessResponse(JDPIEfetivarOrdemPagamentoResponse result, string message, string correlationId)
        {
            return BaseReturn<JDPIEfetivarOrdemPagamentoResponse>.FromSuccess(
                result,
                message,
                correlationId
            );
        }


        public override BaseReturn<JDPIEfetivarOrdemPagamentoResponse> ReturnErrorResponse(Exception exception, string correlationId)
        {
            return BaseReturn<JDPIEfetivarOrdemPagamentoResponse>.FromException(exception, correlationId);
        }

    }
}
