using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Pagamento.CancelarOrdemPagamento
{
    public class CancelarOrdemPagamentoHandler : BSUseCaseHandler<TransactionCancelarOrdemPagamento, BaseReturn<JDPICancelarOrdemPagamentoResponse>, JDPICancelarOrdemPagamentoResponse>
    {

        public CancelarOrdemPagamentoHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionCancelarOrdemPagamento transaction, CancellationToken cancellationToken)
        {
            var errors = new List<ErrorDetails>();

            var clienteValidation = _validateService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
            if (!clienteValidation.IsValid)
                errors.AddRange(clienteValidation.Errors);

            var motivoValidation = _validateService.ValidarMotivo(transaction.motivo);
            if (!motivoValidation.IsValid)
                errors.AddRange(motivoValidation.Errors);

            return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
        }



        protected override async Task<JDPICancelarOrdemPagamentoResponse> ExecuteTransactionProcessing(TransactionCancelarOrdemPagamento transaction, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _spaRepoSql.CancelarOrdemPagamento(transaction);
                return new JDPICancelarOrdemPagamentoResponse(result);
            }
            catch (BusinessException bex)
            {
                _loggingAdapter.LogError("Erro retornado pela Sps", bex);
                throw;
            }
            catch (Exception dbEx)
            {
                _loggingAdapter.LogError("Erro de database durante cancelamento", dbEx);
                throw;
            }
        }


        protected override BaseReturn<JDPICancelarOrdemPagamentoResponse> ReturnSuccessResponse(JDPICancelarOrdemPagamentoResponse result, string message, string correlationId)
        {
            return BaseReturn<JDPICancelarOrdemPagamentoResponse>.FromSuccess(
                result,
                message,
                correlationId
            );
        }



        protected override BaseReturn<JDPICancelarOrdemPagamentoResponse> ReturnErrorResponse(Exception exception, string correlationId)
        {
            return BaseReturn<JDPICancelarOrdemPagamentoResponse>.FromException(exception, correlationId);
        }

    }
}
