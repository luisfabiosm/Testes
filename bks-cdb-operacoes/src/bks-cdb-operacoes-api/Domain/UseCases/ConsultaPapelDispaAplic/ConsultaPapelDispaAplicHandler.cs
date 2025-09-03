using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Models.Transactions;

namespace Domain.UseCases.ConsultaPapelDispaAplic;

public class ConsultaPapelDispaAplicHandler : BSUseCaseHandler<TransactionConsultaPapelDispAplic, BaseReturn<ResponsePapelDispAplic>, ResponsePapelDispAplic>
{
    public ConsultaPapelDispaAplicHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionConsultaPapelDispAplic transaction, CancellationToken cancellationToken)
    {
        var errors = new List<ValidationErrorDetails>();



        return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
    }


    protected override async Task<ResponsePapelDispAplic> ExecuteTransactionProcessing(TransactionConsultaPapelDispAplic transaction, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repo.ExecuteTransaction<ResponsePapelDispAplic>(transaction);

            return new ResponsePapelDispAplic(await HandleProcessingResult(result));

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


    protected override BaseReturn<ResponsePapelDispAplic> ReturnSuccessResponse(ResponsePapelDispAplic result, string message, string correlationId)
    {
        return BaseReturn<ResponsePapelDispAplic>.FromSuccess(
            result,
            message,
            correlationId
        );
    }


    protected override BaseReturn<ResponsePapelDispAplic> ReturnErrorResponse(Exception exception, string correlationId)
    {

        return BaseReturn<ResponsePapelDispAplic>.FromException(exception, correlationId);
    }

}

