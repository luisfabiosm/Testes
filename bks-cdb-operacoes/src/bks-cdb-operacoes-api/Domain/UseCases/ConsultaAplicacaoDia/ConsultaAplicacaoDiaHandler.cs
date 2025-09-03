using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Models.Transactions;

namespace Domain.UseCases.ConsultaAplicacaoDia;

public class ConsultaAplicacaoDiaHandler : BSUseCaseHandler<TransactionConsultaAplicacaoDia, BaseReturn<ResponseAplicacaoNoDia>, ResponseAplicacaoNoDia>
{
    public ConsultaAplicacaoDiaHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionConsultaAplicacaoDia transaction, CancellationToken cancellationToken)
    {
        var errors = new List<ValidationErrorDetails>();

 
        //Criar validação aqui


        return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
    }


    protected override async Task<ResponseAplicacaoNoDia> ExecuteTransactionProcessing(TransactionConsultaAplicacaoDia transaction, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repo.ExecuteTransaction(transaction);

            return new ResponseAplicacaoNoDia(await HandleProcessingResult(result));

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


    protected override BaseReturn<ResponseAplicacaoNoDia> ReturnSuccessResponse(ResponseAplicacaoNoDia result, string message, string correlationId)
    {
        return BaseReturn<ResponseAplicacaoNoDia>.FromSuccess(
            result,
            message,
            correlationId
        );
    }


    protected override BaseReturn<ResponseAplicacaoNoDia> ReturnErrorResponse(Exception exception, string correlationId)
    {

        return BaseReturn<ResponseAplicacaoNoDia>.FromException(exception, correlationId);
    }

}

