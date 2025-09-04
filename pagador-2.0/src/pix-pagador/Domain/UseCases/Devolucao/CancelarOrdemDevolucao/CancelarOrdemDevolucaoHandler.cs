using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Devolucao.CancelarOrdemDevolucao
{
    public class CancelarOrdemDevolucaoHandler : BSUseCaseHandler<TransactionCancelarOrdemDevolucao, BaseReturn<JDPICancelarOrdemDevolucaoResponse>, JDPICancelarOrdemDevolucaoResponse>
    {


        public CancelarOrdemDevolucaoHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }


        protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionCancelarOrdemDevolucao transaction, CancellationToken cancellationToken)
        {
            var errors = new List<ErrorDetails>();

            // Validação de idReqSistemaCliente
            var clienteValidation = _validateService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
            if (!clienteValidation.IsValid)
                errors.AddRange(clienteValidation.Errors);


            return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
        }


        protected override async Task<JDPICancelarOrdemDevolucaoResponse> ExecuteTransactionProcessing(TransactionCancelarOrdemDevolucao transaction, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _spaRepoSql.CancelarOrdemDevolucao(transaction);
                return new JDPICancelarOrdemDevolucaoResponse(result);
            }
            catch (BusinessException bex)
            {
                _loggingAdapter.LogError("Erro retornado pela Sps", bex);
                throw;
            }
            catch (Exception dbEx)
            {
                _loggingAdapter.LogError("Erro de database durante cancelamento de ordem de devolução", dbEx);
                throw;
            }
        }


        protected override BaseReturn<JDPICancelarOrdemDevolucaoResponse> ReturnSuccessResponse(JDPICancelarOrdemDevolucaoResponse result, string message, string correlationId)
        {
            return BaseReturn<JDPICancelarOrdemDevolucaoResponse>.FromSuccess(
                result,
                message,
                correlationId
            );
        }


        protected override BaseReturn<JDPICancelarOrdemDevolucaoResponse> ReturnErrorResponse(Exception exception, string correlationId)
        {
            return BaseReturn<JDPICancelarOrdemDevolucaoResponse>.FromException(exception, correlationId);
        }

    }
}
