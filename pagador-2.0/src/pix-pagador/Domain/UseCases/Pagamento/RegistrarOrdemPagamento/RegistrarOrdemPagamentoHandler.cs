using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Pagamento.RegistrarOrdemPagamento;

public class RegistrarOrdemPagamentoHandler : BSUseCaseHandler<TransactionRegistrarOrdemPagamento, BaseReturn<JDPIRegistrarOrdemPagamentoResponse>, JDPIRegistrarOrdemPagamentoResponse>
{
    public RegistrarOrdemPagamentoHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<ValidationResult> ExecuteSpecificValidations(TransactionRegistrarOrdemPagamento transaction, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetails>();

        // Validação de idReqSistemaCliente
        var clienteValidation = _validateService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
        if (!clienteValidation.IsValid)
            errors.AddRange(clienteValidation.Errors);

        // Validação de pagador
        if (transaction.pagador != null)
        {
            var pagadorValidation = _validateService.ValidarPagador(transaction.pagador);
            if (!pagadorValidation.IsValid)
                errors.AddRange(pagadorValidation.Errors);
        }
        else
        {
            errors.Add(new ErrorDetails("pagador", "Dados do pagador são obrigatórios"));
        }

        // Validação de recebedor
        if (transaction.recebedor != null)
        {
            var recebedorValidation = _validateService.ValidarRecebedor(transaction.recebedor);
            if (!recebedorValidation.IsValid)
                errors.AddRange(recebedorValidation.Errors);
        }
        else
        {
            errors.Add(new ErrorDetails("recebedor", "Dados do recebedor são obrigatórios"));
        }

        // Validação de valor
        var valorValidation = _validateService.ValidarValor(transaction.valor);
        if (!valorValidation.IsValid)
            errors.AddRange(valorValidation.Errors);

        // Validação de tipo de iniciação
        var iniciacaoValidation = _validateService.ValidarTpIniciacao(transaction.tpIniciacao);
        if (!iniciacaoValidation.IsValid)
            errors.AddRange(iniciacaoValidation.Errors);

        // Validações opcionais
        if (transaction.prioridadePagamento.HasValue)
        {
            var prioridadeValidation = _validateService.ValidarPrioridadePagamento(transaction.prioridadePagamento);
            if (!prioridadeValidation.IsValid)
                errors.AddRange(prioridadeValidation.Errors);
        }

        return errors.Count > 0 ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
    }


    protected override async Task<JDPIRegistrarOrdemPagamentoResponse> ExecuteTransactionProcessing(TransactionRegistrarOrdemPagamento transaction, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _spaRepoSql.RegistrarOrdemPagamento(transaction);
            return new JDPIRegistrarOrdemPagamentoResponse(await HandleProcessingResult(result));
        }
        catch (BusinessException bex)
        {
            _loggingAdapter.LogError("Erro retornado pela Sps", bex);
            throw;
        }
        catch (Exception ex)
        {
            _loggingAdapter.LogError("Erro de database durante registro de ordem de pagamento", ex);
            throw;
        }
    }


    protected override BaseReturn<JDPIRegistrarOrdemPagamentoResponse> ReturnSuccessResponse(JDPIRegistrarOrdemPagamentoResponse result, string message, string correlationId)
    {
        return BaseReturn<JDPIRegistrarOrdemPagamentoResponse>.FromSuccess(
            result,
            message,
            correlationId
        );
    }


    protected override BaseReturn<JDPIRegistrarOrdemPagamentoResponse> ReturnErrorResponse(Exception exception, string correlationId)
    {

        return BaseReturn<JDPIRegistrarOrdemPagamentoResponse>.FromException(exception, correlationId);
    }


}

