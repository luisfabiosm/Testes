using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Models.Transactions;

namespace Domain.UseCases.ConsultaCarteiraAplicacao;

public class ConsultaCarteiraAplicacaoHandler : BSUseCaseHandler<TransactionConsultaCarteiraAplicacao, BaseReturn<ResponseCarteira>, ResponseCarteira>
{
    public ConsultaCarteiraAplicacaoHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionConsultaCarteiraAplicacao transaction, CancellationToken cancellationToken)
    {
        var errors = new List<ValidationErrorDetails>();

 
        //Criar validação aqui


        return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
    }


    protected override async Task<ResponseCarteira> ExecuteTransactionProcessing(TransactionConsultaCarteiraAplicacao transaction, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repo.ExecuteTransaction(transaction);

            return new ResponseCarteira(await HandleProcessingResult(result));

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


    protected override BaseReturn<ResponseCarteira> ReturnSuccessResponse(ResponseCarteira result, string message, string correlationId)
    {
        return BaseReturn<ResponseCarteira>.FromSuccess(
            result,
            message,
            correlationId
        );
    }


    protected override BaseReturn<ResponseCarteira> ReturnErrorResponse(Exception exception, string correlationId)
    {

        return BaseReturn<ResponseCarteira>.FromException(exception, correlationId);
    }

}

