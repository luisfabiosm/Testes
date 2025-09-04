using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Devolucao.EfetivarOrdemDevolucao
{
    public class EfetivarOrdemDevolucaoHandler : BSUseCaseHandler<TransactionEfetivarOrdemDevolucao, BaseReturn<JDPIEfetivarOrdemDevolucaoResponse>, JDPIEfetivarOrdemDevolucaoResponse>
    {


        public EfetivarOrdemDevolucaoHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }


        protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionEfetivarOrdemDevolucao transaction, CancellationToken cancellationToken)
        {
            var errors = new List<ErrorDetails>();

            // Validação de idReqSistemaCliente
            var clienteValidation = _validateService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
            if (!clienteValidation.IsValid)
                errors.AddRange(clienteValidation.Errors);

            var endToEndValidation = _validateService.ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal);
            if (!endToEndValidation.IsValid)
                errors.AddRange(endToEndValidation.Errors);



            return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
        }


        protected override async Task<JDPIEfetivarOrdemDevolucaoResponse> ExecuteTransactionProcessing(TransactionEfetivarOrdemDevolucao transaction, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _spaRepoSql.EfetivarOrdemDevolucao(transaction);
                return new JDPIEfetivarOrdemDevolucaoResponse(result);
            }
            catch (BusinessException bex)
            {
                _loggingAdapter.LogError("Erro retornado pela Sps", bex);
                throw;
            }
            catch (Exception dbEx)
            {
                _loggingAdapter.LogError("Erro de database durante efetivacao de ordem de devolução", dbEx);
                throw;
            }
        }


        protected override BaseReturn<JDPIEfetivarOrdemDevolucaoResponse> ReturnSuccessResponse(JDPIEfetivarOrdemDevolucaoResponse result, string message, string correlationId)
        {
            return BaseReturn<JDPIEfetivarOrdemDevolucaoResponse>.FromSuccess(
                result,
                message,
                correlationId
            );
        }

        protected override BaseReturn<JDPIEfetivarOrdemDevolucaoResponse> ReturnErrorResponse(Exception exception, string correlationId)
        {
            return BaseReturn<JDPIEfetivarOrdemDevolucaoResponse>.FromException(exception, correlationId);
        }


    }
}
