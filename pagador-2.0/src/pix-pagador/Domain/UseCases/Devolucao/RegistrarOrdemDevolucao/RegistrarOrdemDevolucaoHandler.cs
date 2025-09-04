using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Devolucao.RegistrarOrdemDevolucao
{
    public class RegistrarOrdemDevolucaoHandler : BSUseCaseHandler<TransactionRegistrarOrdemDevolucao, BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>, JDPIRegistrarOrdemDevolucaoResponse>
    {
        public RegistrarOrdemDevolucaoHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionRegistrarOrdemDevolucao transaction, CancellationToken cancellationToken)
        {
            var errors = new List<ErrorDetails>();

            // Validação de idReqSistemaCliente
            var clienteValidation = _validateService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
            if (!clienteValidation.IsValid)
                errors.AddRange(clienteValidation.Errors);

            var endToEndValidation = _validateService.ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal);
            if (!endToEndValidation.IsValid)
                errors.AddRange(endToEndValidation.Errors);

            var codigoDevolucaoValidation = _validateService.ValidarCodigoDevolucao(transaction.codigoDevolucao);
            if (!codigoDevolucaoValidation.IsValid)
                errors.AddRange(codigoDevolucaoValidation.Errors);

            var valorDevolucaoValidation = _validateService.ValidarValor(transaction.valorDevolucao);
            if (!valorDevolucaoValidation.IsValid)
                errors.AddRange(valorDevolucaoValidation.Errors);

            return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
        }


        protected override async Task<JDPIRegistrarOrdemDevolucaoResponse> ExecuteTransactionProcessing(TransactionRegistrarOrdemDevolucao transaction, CancellationToken cancellationToken)
        {

            try
            {
                var result = await _spaRepoSql.RegistrarOrdemDevolucao(transaction);
                return new JDPIRegistrarOrdemDevolucaoResponse(result);
            }
            catch (BusinessException bex)
            {
                _loggingAdapter.LogError("Erro retornado pela Sps", bex);
                throw;
            }
            catch (Exception dbEx)
            {
                _loggingAdapter.LogError("Erro de database durante registro de ordem de devolução", dbEx);
                throw;
            }
        }


        protected override BaseReturn<JDPIRegistrarOrdemDevolucaoResponse> ReturnSuccessResponse(JDPIRegistrarOrdemDevolucaoResponse result, string message, string correlationId)
        {
            return BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>.FromSuccess(
                result,
                message,
                correlationId
            );
        }


        protected override BaseReturn<JDPIRegistrarOrdemDevolucaoResponse> ReturnErrorResponse(Exception exception, string correlationId)
        {

            return BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>.FromException(exception, correlationId);
        }

    }
}
