using Domain.Core.Common.Base;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Exceptions;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;

namespace Domain.Core.Common.Mediator
{

    public abstract class BSUseCaseHandler<TTransaction, TResponse, TResult> : BaseService, IBSRequestHandler<TTransaction, TResponse>
        where TTransaction : BaseTransaction<TResponse>
        where TResponse : BaseReturn<TResult>
    {
        //protected readonly IValidatorService _validateService;
        protected readonly ISPARepository _repo;

        protected BSUseCaseHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _repo = serviceProvider.GetRequiredService<ISPARepository>();
            //_validateService = serviceProvider.GetRequiredService<IValidatorService>();
        }

        public async Task<TResponse> Handle(TTransaction transaction, CancellationToken cancellationToken)
        {
            var correlationId = transaction.CorrelationId;

            _loggingAdapter.LogInformation(
                "Iniciando processamento: {RequestType} [CorrelationId: {CorrelationId}]",
                typeof(TTransaction).Name,
                correlationId);

            try
            {
                // 1. Validação usando Result Pattern
                var validationResult = await ValidateTransaction(transaction, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.mensagem));
                    _loggingAdapter.LogWarning("Falha na validação: {ValidationErrors} [CorrelationId: {CorrelationId}]",
                        errorMessage, correlationId);
                    return ReturnValidationError(validationResult, correlationId);
                }

                // 2. Pré-processamento
                await PreProcessing(transaction, cancellationToken);

                // 3. Processamento principal
                var processingResult = await ExecuteTransactionProcessing(transaction, cancellationToken);
                if (processingResult == null)
                {
                    _loggingAdapter.LogError("Processamento retornou resultado nulo [CorrelationId: {CorrelationId}]");
                    return ReturnErrorResponse(new InvalidOperationException("Processamento falhou"), correlationId);
                }

                // 4. Pós-processamento
                var response = ReturnSuccessResponse(processingResult, "Processamento concluído com sucesso", correlationId);
                await PosProcessing(transaction, response, cancellationToken);

                _loggingAdapter.LogInformation(
                    "Processamento concluído com sucesso: {RequestType} [CorrelationId: {CorrelationId}]",
                    typeof(TTransaction).Name,
                    correlationId);

                return response;
            }
            catch (BusinessException bex)
            {

                _loggingAdapter.LogError(
                    "Erro retornado Sps: {RequestType} [CorrelationId: {CorrelationId}]", bex,
                    typeof(TTransaction).Name,
                    correlationId);

                return await HandleBusinessError("Handle", transaction, bex, cancellationToken);
            }
            catch (Exception ex)
            {
                // Exceptions são tratadas apenas para erros não esperados
                _loggingAdapter.LogError(
                    "Erro não esperado durante processamento: {RequestType} [CorrelationId: {CorrelationId}]", ex,
                    typeof(TTransaction).Name,
                    correlationId);

                return await HandleUnexpectedError("Handle", transaction, ex, cancellationToken);
            }
        }

        protected virtual async Task<ValidationResult> ValidateTransaction(TTransaction transaction, CancellationToken cancellationToken)
        {
            var errors = new List<ValidationErrorDetails>();

            // Validações básicas
            if (transaction.Code <= 0)
                errors.Add(new ValidationErrorDetails("Code", "Code é obrigatório e deve ser maior que 0"));

            if (string.IsNullOrWhiteSpace(transaction.CorrelationId))
                errors.Add(new ValidationErrorDetails("CorrelationId", "CorrelationId é obrigatório"));

            // Se há erros básicos, retorna imediatamente
            if (errors.Count > 0)
                return ValidationResult.Invalid(errors);

            // Executar validações específicas da transação
            return await ExecuteSpecificValidations(transaction, cancellationToken);
        }

        protected abstract Task<ValidationResult> ExecuteSpecificValidations(TTransaction transaction, CancellationToken cancellationToken);

        protected abstract Task<TResult> ExecuteTransactionProcessing(TTransaction transaction, CancellationToken cancellationToken);

        protected abstract TResponse ReturnSuccessResponse(TResult result, string message, string correlationId);

        protected abstract TResponse ReturnErrorResponse(Exception exception, string correlationId);

        protected virtual TResponse ReturnValidationError(ValidationResult validation, string correlationId)
        {
            var validationException =  ValidateException.Create(validation.Errors);

            return ReturnErrorResponse(validationException, correlationId);
        }

        protected virtual Task PreProcessing(TTransaction transaction, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task PosProcessing(TTransaction transaction, TResponse response, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task<TResponse> HandleBusinessError(string operation, TTransaction transaction, BusinessException exception, CancellationToken cancellationToken)
        {
            _loggingAdapter.LogError("Erro de Negocio retornado pela Sps em {Operation}", exception, operation);
            return ReturnErrorResponse(exception, transaction.CorrelationId);
        }

        protected virtual async Task<TResponse> HandleUnexpectedError(string operation, TTransaction transaction, Exception exception, CancellationToken cancellationToken)
        {
            _loggingAdapter.LogError("Erro não esperado em {Operation}", exception, operation);
            return ReturnErrorResponse(exception, transaction.CorrelationId);
        }

        protected virtual async Task<string> HandleProcessingResult(string result, Exception exception = null)
        {
            if (exception != null)
            {
                _loggingAdapter.LogError("Erro durante processamento: {Result}", exception, result);
                throw exception;
            }

            if (result.IndexOf("codErro") > 0)
            {
                var _bError = ErrorDetailsReturn.Create(result);
                throw BusinessException.Create(_bError);
            }

            //Ok
            return result;
        }
    }
}