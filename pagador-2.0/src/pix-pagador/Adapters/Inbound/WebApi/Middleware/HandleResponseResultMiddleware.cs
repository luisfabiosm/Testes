using Domain.Core.Enum;
using Domain.Core.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace Adapters.Inbound.WebApi.Middleware
{
    /// <summary>
    /// Middleware para padronização de respostas baseado no padrão Result.
    /// Centraliza a lógica de conversão do Result para IActionResult, garantindo consistência em todas as respostas da API.
    /// </summary>
    public class HandleResponseResultMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HandleResponseResultMiddleware> _logger;

        public HandleResponseResultMiddleware(RequestDelegate next, ILogger<HandleResponseResultMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                EnsureCorrelationId(context);

                if (ShouldInterceptEndpoint(context))
                {
                    await HandleResultEndpointAsync(context);
                    return;
                }

                await _next(context);
            }
            //catch (BusinessException bex)
            //{
            //    _logger.LogWarning(bex, "BusinessException capturada no middleware [CorrelationId: {CorrelationId}]",
            //        GetCorrelationId(context));
            //    await HandleBusinessExceptionAsync(context, bex);
            //}
            //catch (ValidateException velx)
            //{
            //    _logger.LogWarning(velx, "ValidateException capturada no middleware [CorrelationId: {CorrelationId}]",
            //        GetCorrelationId(context));
            //    await HandleValidateExceptionAsync(context, velx);
            //}
            //catch (ValidationException vex)
            //{
            //    _logger.LogWarning(vex, "ValidationException capturada no middleware [CorrelationId: {CorrelationId}]",
            //        GetCorrelationId(context));
            //    await HandleValidationExceptionAsync(context, vex);
            //}
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não esperado capturado no middleware [CorrelationId: {CorrelationId}]",
                    GetCorrelationId(context));
                await HandleGenericExceptionAsync(context, ex);
            }
        }

        private void EnsureCorrelationId(HttpContext context)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;
        }

        private string GetCorrelationId(HttpContext context)
        {
            return context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        }

        private static bool ShouldInterceptEndpoint(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/soa/pix/api/v1");
        }

        private async Task HandleResultEndpointAsync(HttpContext context)
        {
            var originalResponseStream = context.Response.Body;

            try
            {
                using var responseStream = new MemoryStream();
                context.Response.Body = responseStream;

                await _next(context);
                await ProcessResultResponseAsync(context, responseStream, originalResponseStream);
            }
            finally
            {
                context.Response.Body = originalResponseStream;
            }
        }

        private async Task ProcessResultResponseAsync(HttpContext context, MemoryStream responseStream, Stream originalResponseStream)
        {
            responseStream.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(responseStream).ReadToEndAsync();

            try
            {
                if (context.Response.StatusCode >= 400)
                {
                    responseStream.Seek(0, SeekOrigin.Begin);
                    await responseStream.CopyToAsync(originalResponseStream);
                    return;
                }

                await TryProcessBaseReturnAsync(context, responseContent, originalResponseStream);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Falha ao processar resposta JSON como BaseReturn. Retornando resposta original.");
                responseStream.Seek(0, SeekOrigin.Begin);
                await responseStream.CopyToAsync(originalResponseStream);
            }
        }

        private async Task TryProcessBaseReturnAsync(HttpContext context, string responseContent, Stream originalResponseStream)
        {
            if (string.IsNullOrEmpty(responseContent))
            {
                await WriteEmptyResponseAsync(context, originalResponseStream);
                return;
            }

            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            if (HasBaseReturnStructure(root))
            {
                await ProcessBaseReturnJsonAsync(context, root, originalResponseStream);
                return;
            }

            await WriteSuccessResponseAsync(context, responseContent, originalResponseStream);
        }

        private static bool HasBaseReturnStructure(JsonElement root)
        {
            bool hasCorrelationId = root.TryGetProperty("correlationId", out _) ||
                                   root.TryGetProperty("CorrelationId", out _);

            bool hasSuccess = root.TryGetProperty("success", out _) ||
                             root.TryGetProperty("Success", out _);

            bool hasErrorCode = root.TryGetProperty("errorCode", out _) ||
                               root.TryGetProperty("ErrorCode", out _);

            return hasCorrelationId && hasSuccess && hasErrorCode;
        }

        private async Task ProcessBaseReturnJsonAsync(HttpContext context, JsonElement root, Stream originalResponseStream)
        {
            var correlationId = GetCorrelationIdFromJson(root) ?? GetCorrelationId(context);
            var isSuccess = GetSuccessFromJson(root);
            var errorCode = GetErrorCodeFromJson(root);

            if (isSuccess)
            {
                var dataElement = GetDataFromJson(root);
                await WriteResultSuccessAsync(context, dataElement, correlationId, originalResponseStream);
                return;
            }

            var error = GetErrorMessageFromJson(root);
            var errorDetailsElement = GetErrorDetailsFromJson(root);
            await WriteResultFailureAsync(context, error, errorCode, correlationId, errorDetailsElement, originalResponseStream);
        }

        private static string? GetCorrelationIdFromJson(JsonElement root)
        {
            if (root.TryGetProperty("correlationId", out var correlProp) ||
                root.TryGetProperty("CorrelationId", out correlProp))
            {
                return correlProp.GetString();
            }
            return null;
        }

        private static bool GetSuccessFromJson(JsonElement root)
        {
            if (root.TryGetProperty("success", out var successProp) ||
                root.TryGetProperty("Success", out successProp))
            {
                return successProp.GetBoolean();
            }
            return false;
        }

        private static int GetErrorCodeFromJson(JsonElement root)
        {
            if (root.TryGetProperty("errorCode", out var errorCodeProp) ||
                root.TryGetProperty("ErrorCode", out errorCodeProp))
            {
                return errorCodeProp.GetInt32();
            }
            return 0;
        }

        private static JsonElement GetDataFromJson(JsonElement root)
        {
            if (root.TryGetProperty("data", out var dataElement) ||
                root.TryGetProperty("Data", out dataElement))
            {
                return dataElement;
            }
            return default;
        }

        private static string GetErrorMessageFromJson(JsonElement root)
        {
            if (root.TryGetProperty("message", out var msgProp) ||
                root.TryGetProperty("Message", out msgProp))
            {
                return msgProp.GetString() ?? "Erro desconhecido";
            }
            return "Erro desconhecido";
        }

        private static JsonElement GetErrorDetailsFromJson(JsonElement root)
        {
            if (root.TryGetProperty("errorDetails", out var errorDetailsElement) ||
                root.TryGetProperty("ErrorDetails", out errorDetailsElement))
            {
                return errorDetailsElement;
            }
            return default;
        }

        private async Task WriteResultSuccessAsync(HttpContext context, JsonElement data, string correlationId, Stream originalResponseStream)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";

            var responseData = data.ValueKind != JsonValueKind.Undefined ? data : (object?)null;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var responseBytes = JsonSerializer.SerializeToUtf8Bytes(responseData, jsonOptions);
            await originalResponseStream.WriteAsync(responseBytes);
        }

        private async Task WriteResultFailureAsync(HttpContext context, string error, int errorCode, string correlationId, JsonElement errorDetailsElement, Stream originalResponseStream)
        {
            var statusCode = errorCode == 400 ? 400 : 500;
            context.Response.StatusCode = statusCode;

            if (errorDetailsElement.ValueKind == JsonValueKind.Undefined || errorCode == 400 || errorCode == -1)
            {

                if (HasValidateExceptionStructure(errorDetailsElement))
                {
                    await WriteValidateExceptionResponseAsync(context, errorDetailsElement, originalResponseStream);
                    return;
                }

                if (HasBusinnesExceptionStructure(errorDetailsElement))
                {
                    await WriteBusinessExceptionResponseAsync(context, errorDetailsElement, originalResponseStream);
                    return;
                }
            }
            await WriteStandardErrorResponseAsync(context, error, statusCode, correlationId, originalResponseStream);
            return;

        }

        private static bool HasBusinnesExceptionStructure(JsonElement errorDetailsElement)
        {
            return errorDetailsElement.TryGetProperty("msgErro", out var msgerrorsElement) &&
                   errorDetailsElement.TryGetProperty("codErro", out var coderrorsElement) &&
                   errorDetailsElement.TryGetProperty("tipoErro", out var tipoerrorsElement);
        }


        private static bool HasValidateExceptionStructure(JsonElement errorDetailsElement)
        {
            return errorDetailsElement.TryGetProperty("RequestErrors", out var errorsElement) &&
                   errorsElement.ValueKind == JsonValueKind.Array;
        }

        private async Task WriteBusinessExceptionResponseAsync(HttpContext context, JsonElement errorDetailsElement, Stream originalResponseStream)
        {
            context.Response.ContentType = "application/json";

            var businessErrorResponse = new Dictionary<string, object>();

            ExtractBusinessErrorProperty(errorDetailsElement, "tipoErro", "tipo", businessErrorResponse);
            ExtractBusinessErrorProperty(errorDetailsElement, "codErro", "codigo", businessErrorResponse);
            ExtractBusinessErrorProperty(errorDetailsElement, "msgErro", "mensagem", businessErrorResponse);
            ExtractBusinessErrorProperty(errorDetailsElement, "origemErro", "origem", businessErrorResponse);

            var responseBytes = SerializeToJson(businessErrorResponse);
            await originalResponseStream.WriteAsync(responseBytes);
        }

        private static void ExtractBusinessErrorProperty(JsonElement errorDetailsElement, string sourceKey, string targetKey, Dictionary<string, object> response)
        {
            if (!errorDetailsElement.TryGetProperty(sourceKey, out var prop))
                return;

            response[targetKey] = sourceKey.Contains("Erro") && sourceKey != "msgErro" && sourceKey != "origemErro" ? prop.GetInt32() : prop.GetString();
        }

        private async Task WriteStandardErrorResponseAsync(HttpContext context, string error, int statusCode, string correlationId, Stream originalResponseStream)
        {
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new
            {
                detail = error,
                status = statusCode,
                title = "Erro no processamento",
                extensions = new Dictionary<string, object?>
                {
                    ["correlationId"] = correlationId
                }
            };

            var responseBytes = SerializeToJson(problemDetails);
            await originalResponseStream.WriteAsync(responseBytes);
        }

        private async Task WriteSuccessResponseAsync(HttpContext context, string responseContent, Stream originalResponseStream)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";

            var responseBytes = Encoding.UTF8.GetBytes(responseContent);
            await originalResponseStream.WriteAsync(responseBytes);
        }

        private async Task WriteEmptyResponseAsync(HttpContext context, Stream originalResponseStream)
        {
            context.Response.StatusCode = 204; // No Content
        }

        private async Task HandleBusinessExceptionAsync(HttpContext context, BusinessException bex)
        {
            var correlationId = GetCorrelationId(context);
            context.Response.StatusCode = 400;

            if (bex.BusinessError != null)
            {
                await WriteBusinessExceptionWithSpsErrorAsync(context, bex);
                return;
            }

            await WriteProblemDetailsResponseAsync(context, bex.Message, 400, correlationId);
        }

        private async Task WriteBusinessExceptionWithSpsErrorAsync(HttpContext context, BusinessException bex)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                tipo = bex.BusinessError!.tipoErro,
                codigo = bex.BusinessError.codErro,
                mensagem = bex.BusinessError.msgErro,
                origem = bex.BusinessError.origemErro
            };

            var responseBytes = SerializeToJson(response);
            await context.Response.Body.WriteAsync(responseBytes);
        }

        private async Task HandleValidateExceptionAsync(HttpContext context, ValidateException vex)
        {
            var correlationId = GetCorrelationId(context);
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            var response = CreateValidateExceptionResponse(vex);
            var responseBytes = SerializeToJson(response);
            await context.Response.Body.WriteAsync(responseBytes);
        }

        private static object CreateValidateExceptionResponse(ValidateException vex)
        {
            if (vex.RequestErrors != null && vex.RequestErrors.Any())
            {
                return new
                {
                    tipo = (int)EnumTipoErro.SISTEMA,
                    codigo = vex.ErrorCode,
                    mensagem = vex.RequestErrors.Select(e => new {
                        campo = e.campo,
                        mensagem = e.mensagens
                    }).ToList(),
                    origem = "pix-pagador -VALIDACAO ENTRADA"
                };
            }

            return new
            {
                tipo = (int)EnumTipoErro.SISTEMA,
                codigo = vex.ErrorCode,
                mensagem = vex.Message,
                origem = "pix-pagador -VALIDACAO ENTRADA"
            };
        }

        private async Task WriteValidateExceptionResponseAsync(HttpContext context, JsonElement errorDetailsElement, Stream originalResponseStream)
        {
            context.Response.ContentType = "application/json";

            var validateErrorResponse = CreateValidateErrorResponse(errorDetailsElement);
            var responseBytes = SerializeToJson(validateErrorResponse);
            await originalResponseStream.WriteAsync(responseBytes);
        }

        private static Dictionary<string, object> CreateValidateErrorResponse(JsonElement errorDetailsElement)
        {
            var validateErrorResponse = new Dictionary<string, object>
            {
                ["tipo"] = (int)EnumTipoErro.SISTEMA,
                ["codigo"] = -1,
                ["origem"] = "pix-pagador -VALIDACAO REQUEST"
            };

            if (errorDetailsElement.TryGetProperty("errors", out var errorsElement) &&
                errorsElement.ValueKind == JsonValueKind.Array)
            {
                var errorList = errorsElement.EnumerateArray()
                    .Select(CreateValidateErrorItem)
                    .ToList();

                validateErrorResponse["mensagem"] = errorList;
                return validateErrorResponse;
            }

            validateErrorResponse["mensagem"] = "Erro de validação";
            return validateErrorResponse;
        }

        private static Dictionary<string, object> CreateValidateErrorItem(JsonElement error)
        {
            var errorObj = new Dictionary<string, object>();

            if (error.TryGetProperty("campo", out var campoElement))
                errorObj["campo"] = campoElement.GetString()!;

            if (error.TryGetProperty("mensagens", out var mensagemElement))
                errorObj["mensagem"] = mensagemElement.GetString()!;

            return errorObj;
        }

        private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException vex)
        {
            var correlationId = GetCorrelationId(context);
            await WriteProblemDetailsResponseAsync(context, vex.Message, 400, correlationId);
        }

        private async Task HandleGenericExceptionAsync(HttpContext context, Exception ex)
        {
            var correlationId = GetCorrelationId(context);
            await WriteProblemDetailsResponseAsync(context, "Erro interno do servidor", 500, correlationId);
        }

        private static async Task WriteProblemDetailsResponseAsync(HttpContext context, string detail, int statusCode, string correlationId)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new
            {
                detail,
                status = statusCode,
                title = "Erro no processamento",
                extensions = new Dictionary<string, object?>
                {
                    ["correlationId"] = correlationId
                }
            };

            var responseBytes = SerializeToJson(problemDetails);
            await context.Response.Body.WriteAsync(responseBytes);
        }

        private static byte[] SerializeToJson(object obj)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            return JsonSerializer.SerializeToUtf8Bytes(obj, jsonOptions);
        }
    }

    public static class HandleResponseResultMiddlewareExtensions
    {
        public static IApplicationBuilder UseHandleResponseResultMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HandleResponseResultMiddleware>();
        }
    }
}