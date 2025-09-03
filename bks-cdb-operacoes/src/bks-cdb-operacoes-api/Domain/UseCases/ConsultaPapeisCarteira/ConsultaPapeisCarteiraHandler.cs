using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Models.Transactions;
using Domain.Core.Ports.Outbound;

namespace Domain.UseCases.ConsultaPapeisCarteira
{


    public class ConsultaPapeisCarteiraHandler : BSUseCaseHandler<TransactionConsultaPapeisCarteira, BaseReturn<ResponsePapeisPorCarteira>, ResponsePapeisPorCarteira>
    {
        public ConsultaPapeisCarteiraHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionConsultaPapeisCarteira transaction, CancellationToken cancellationToken)
        {
            var errors = new List<ValidationErrorDetails>();


            //Criar validação aqui


            return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
        }


        protected override async Task<ResponsePapeisPorCarteira> ExecuteTransactionProcessing(TransactionConsultaPapeisCarteira transaction, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _repo.ExecuteTransaction(transaction);

                return new ResponsePapeisPorCarteira(await HandleProcessingResult(result));

            }
            catch (BusinessException bex)
            {
                _loggingAdapter.LogError("Erro retornado pela Sps", bex);
                throw;
            }
            catch (Exception ex)
            {
                _loggingAdapter.LogError("Erro inesperado de database", ex);
                throw;
            }
        }


        protected override BaseReturn<ResponsePapeisPorCarteira> ReturnSuccessResponse(ResponsePapeisPorCarteira result, string message, string correlationId)
        {
            return BaseReturn<ResponsePapeisPorCarteira>.FromSuccess(
                result,
                message,
                correlationId
            );
        }


        protected override BaseReturn<ResponsePapeisPorCarteira> ReturnErrorResponse(Exception exception, string correlationId)
        {

            return BaseReturn<ResponsePapeisPorCarteira>.FromException(exception, correlationId);
        }

    }


}
