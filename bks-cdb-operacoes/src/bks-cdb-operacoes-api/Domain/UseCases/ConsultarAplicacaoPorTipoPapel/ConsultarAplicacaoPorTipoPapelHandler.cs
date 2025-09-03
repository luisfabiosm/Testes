using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Models.Transactions;

namespace Domain.UseCases.ConsultarAplicacaoPorTipoPapel;

public class ConsultarAplicacaoPorTipoPapelHandler : BSUseCaseHandler<TransactionConsultarAplicacaoPorTipoPapel, BaseReturn<ResponseAplicacaoPorTipoPapel>, ResponseAplicacaoPorTipoPapel>
{
    public ConsultarAplicacaoPorTipoPapelHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionConsultarAplicacaoPorTipoPapel transaction, CancellationToken cancellationToken)
    {
        var errors = new List<ValidationErrorDetails>();

 
        //Criar validação aqui


        return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
    }


    protected override async Task<ResponseAplicacaoPorTipoPapel> ExecuteTransactionProcessing(TransactionConsultarAplicacaoPorTipoPapel transaction, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repo.ExecuteTransaction(transaction);

            return new ResponseAplicacaoPorTipoPapel(await HandleProcessingResult(result));

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


    protected override BaseReturn<ResponseAplicacaoPorTipoPapel> ReturnSuccessResponse(ResponseAplicacaoPorTipoPapel result, string message, string correlationId)
    {
        return BaseReturn<ResponseAplicacaoPorTipoPapel>.FromSuccess(
            result,
            message,
            correlationId
        );
    }


    protected override BaseReturn<ResponseAplicacaoPorTipoPapel> ReturnErrorResponse(Exception exception, string correlationId)
    {

        return BaseReturn<ResponseAplicacaoPorTipoPapel>.FromException(exception, correlationId);
    }

}

