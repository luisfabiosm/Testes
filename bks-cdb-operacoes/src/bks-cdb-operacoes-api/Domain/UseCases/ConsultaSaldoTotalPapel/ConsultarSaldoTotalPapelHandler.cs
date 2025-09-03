using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Models.Transactions;

namespace Domain.UseCases.ConsultaSaldoTotalPapel;

public class ConsultarSaldoTotalPapelHandler : BSUseCaseHandler<TransactionConsultaSaldoTotalPapel, BaseReturn<ResponseSaldoPorTipoPapel>, ResponseSaldoPorTipoPapel>
{
    public ConsultarSaldoTotalPapelHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionConsultaSaldoTotalPapel transaction, CancellationToken cancellationToken)
    {
        var errors = new List<ValidationErrorDetails>();

 
        //Criar validação aqui


        return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
    }


    protected override async Task<ResponseSaldoPorTipoPapel> ExecuteTransactionProcessing(TransactionConsultaSaldoTotalPapel transaction, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repo.ExecuteTransaction(transaction);

            return new ResponseSaldoPorTipoPapel(await HandleProcessingResult(result));

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


    protected override BaseReturn<ResponseSaldoPorTipoPapel> ReturnSuccessResponse(ResponseSaldoPorTipoPapel result, string message, string correlationId)
    {
        return BaseReturn<ResponseSaldoPorTipoPapel>.FromSuccess(
            result,
            message,
            correlationId
        );
    }


    protected override BaseReturn<ResponseSaldoPorTipoPapel> ReturnErrorResponse(Exception exception, string correlationId)
    {

        return BaseReturn<ResponseSaldoPorTipoPapel>.FromException(exception, correlationId);
    }

}

