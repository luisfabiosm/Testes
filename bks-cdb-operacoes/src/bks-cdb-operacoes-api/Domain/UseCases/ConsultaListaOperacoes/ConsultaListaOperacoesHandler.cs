using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Models.Transactions;
using Domain.Core.Ports.Outbound;

namespace Domain.UseCases.ConsultaListaOperacoes
{
 
    public class ConsultaListaOperacoesHandler : BSUseCaseHandler<TransactionConsultaListaOperacoes, BaseReturn<ResponseTipoOperacao>, ResponseTipoOperacao>
    {
        public ConsultaListaOperacoesHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionConsultaListaOperacoes transaction, CancellationToken cancellationToken)
        {
            var errors = new List<ValidationErrorDetails>();


            //Criar validação aqui


            return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
        }


        protected override async Task<ResponseTipoOperacao> ExecuteTransactionProcessing(TransactionConsultaListaOperacoes transaction, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _repo.ExecuteTransaction(transaction);

                return new ResponseTipoOperacao(await HandleProcessingResult(result));

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


        protected override BaseReturn<ResponseTipoOperacao> ReturnSuccessResponse(ResponseTipoOperacao result, string message, string correlationId)
        {
            return BaseReturn<ResponseTipoOperacao>.FromSuccess(
                result,
                message,
                correlationId
            );
        }


        protected override BaseReturn<ResponseTipoOperacao> ReturnErrorResponse(Exception exception, string correlationId)
        {

            return BaseReturn<ResponseTipoOperacao>.FromException(exception, correlationId);
        }

    }



}
