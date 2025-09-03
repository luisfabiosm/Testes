using Domain.Core.Common.Serialization;
using Domain.Core.Enum;
using Domain.Core.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Runtime;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro capturado no middleware [CorrelationId: {CorrelationId}]",
                    GetCorrelationId(context));
                await HandleExceptionAsync(context, ex);
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
            var statusCode = errorCode == 400 || errorCode == -1 ? 400 : 500;
            context.Response.StatusCode = statusCode;

            if (errorDetailsElement.ValueKind == JsonValueKind.Undefined || errorCode == 400 || errorCode == -1)
            {
                if (errorDetailsElement.TryGetProperty("validationErrors", out var validationErrorsProp) ||
                     errorDetailsElement.TryGetProperty("ValidationErrors", out validationErrorsProp))
                {
                    await WriteValidateExceptionFromBaseReturnAsync(context, validationErrorsProp, originalResponseStream);
                    return;
                }
        
                if (errorDetailsElement.TryGetProperty("tipoErro", out _) ||
                    errorDetailsElement.TryGetProperty("codErro", out _))
                {
                    await WriteBusinessExceptionResponseAsync(context, errorDetailsElement, originalResponseStream);
                    return;
                }

            }

            if (errorDetailsElement.ValueKind == JsonValueKind.Undefined)
            {
                await WriteDefault400ExceptionResponseAsync(context, error, errorCode, correlationId, originalResponseStream);
                return;
            }

            await WriteStandardErrorResponseAsync(context, error, statusCode, correlationId, originalResponseStream);
            return;

        }

        private async Task WriteValidateExceptionFromBaseReturnAsync(HttpContext context, JsonElement validationErrorsElement, Stream originalResponseStream)
        {
            context.Response.ContentType = "application/json";

            object validateErrorResponse;

            if (validationErrorsElement.ValueKind == JsonValueKind.Array)
            {
                var errorList = new List<object>();
                foreach (var error in validationErrorsElement.EnumerateArray())
                {
                    var errorObj = new
                    {
                        campo = error.TryGetProperty("campo", out var campoElement) ? campoElement.GetString() : "",
                        mensagem = error.TryGetProperty("mensagem", out var mensagemElement) ? mensagemElement.GetString() : ""
                    };
                    errorList.Add(errorObj);
                }

                validateErrorResponse = new
                {
                    tipo = (int)EnumTipoErro.SISTEMA,
                    codigo = -1,
                    mensagem = errorList.ToArray(),
                    origem = "pix-pagador - VALIDACAO REQUEST"
                };
            }
            else
            {
                validateErrorResponse = new
                {
                    tipo = (int)EnumTipoErro.SISTEMA,
                    codigo = -1,
                    mensagem = "Erro de validação",
                    origem = "APIPixPagador -VALIDACAO ENTRADA"
                };
            }

            var responseBytes = SerializeToJson(validateErrorResponse);
            await originalResponseStream.WriteAsync(responseBytes);
        }

        private async Task WriteBusinessExceptionResponseAsync(HttpContext context, JsonElement errorDetailsElement, Stream originalResponseStream)
        {
            context.Response.ContentType = "application/json";

            var businessErrorResponse = new
            {
                tipo = errorDetailsElement.TryGetProperty("tipoErro", out var tipoErroProp) ? tipoErroProp.GetInt32() : (int)EnumTipoErro.NEGOCIO,
                codigo = errorDetailsElement.TryGetProperty("codErro", out var codErroProp) ? codErroProp.GetInt32() : 400,
                mensagem = errorDetailsElement.TryGetProperty("msgErro", out var msgErroProp) ? msgErroProp.GetString() : "Erro de negócio",
                origem = errorDetailsElement.TryGetProperty("origemErro", out var origemErroProp) ? origemErroProp.GetString() : "pix-pagador"
            };

            var responseBytes = SerializeToJson(businessErrorResponse);
            await originalResponseStream.WriteAsync(responseBytes);
        }
      
        private async Task WriteDefault400ExceptionResponseAsync(HttpContext context, string error, int statusCode, string correlationId, Stream originalResponseStream)
        {
            context.Response.ContentType = "application/json";

            var problemDetails = new
            {
                tipo = (statusCode == -1) ? (int)EnumTipoErro.SISTEMA : (int)EnumTipoErro.NEGOCIO,
                codigo = statusCode,
                mensagem = error,
                origem = "pix-pagador"
            };

            var responseBytes = SerializeToJson(problemDetails);
            await originalResponseStream.WriteAsync(responseBytes);
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

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var correlationId = GetCorrelationId(context);

            switch (ex)
            {
                case ValidateException vex:
                    _logger.LogWarning(vex, "ValidateException tratada no HandleExceptionAsync [CorrelationId: {CorrelationId}]", correlationId);
                    await WriteValidateExceptionDirectAsync(context, vex, correlationId);
                    break;

                case BusinessException bex:
                    _logger.LogWarning(bex, "BusinessException tratada no HandleExceptionAsync [CorrelationId: {CorrelationId}]", correlationId);
                    await WriteBusinessExceptionDirectAsync(context, bex, correlationId);
                    break;

                case ValidationException vex:
                    _logger.LogWarning(vex, "ValidationException tratada no HandleExceptionAsync [CorrelationId: {CorrelationId}]", correlationId);
                    await WriteValidationExceptionDirectAsync(context, vex, correlationId);
                    break;

                default:
                    _logger.LogError(ex, "Exception genérica tratada no HandleExceptionAsync [CorrelationId: {CorrelationId}]", correlationId);
                    await WriteGenericExceptionResponseAsync(context, ex, correlationId);
                    break;
            }
        }

        private async Task WriteGenericExceptionResponseAsync(HttpContext context, Exception ex, string correlationId)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = new
            {
                tipo = (int)EnumTipoErro.SISTEMA,
                codigo = 500,
                mensagem = "Erro interno do servidor",
                origem = "pix-pagador"
            };

            var responseBytes = SerializeToJson(response);
            await context.Response.Body.WriteAsync(responseBytes);
        }

        private async Task WriteValidateExceptionDirectAsync(HttpContext context, ValidateException vex, string correlationId)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            object response;

            if (vex.RequestErrors != null && vex.RequestErrors.Any())
            {
                response = new
                {
                    tipo = (int)EnumTipoErro.SISTEMA,
                    codigo = vex.ErrorCode,
                    mensagem = vex.RequestErrors.Select(e => new {
                        campo = e.campo,
                        mensagem = e.mensagem
                    }).ToArray(),
                    origem = "pix-pagador -VALIDACAO ENTRADA"
                };
            }
            else
            {
                response = new
                {
                    tipo = (int)EnumTipoErro.SISTEMA,
                    codigo = vex.ErrorCode,
                    mensagem = vex.Message,
                    origem = "pix-pagador -VALIDACAO ENTRADA"
                };
            }

            var responseBytes = SerializeToJson(response);
            await context.Response.Body.WriteAsync(responseBytes);
        }

        private async Task WriteBusinessExceptionDirectAsync(HttpContext context, BusinessException bex, string correlationId)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            object response;
            if (bex.BusinessError != null)
            {
                response = new
                {
                    tipo = bex.BusinessError.tipoErro,
                    codigo = bex.BusinessError.codErro,
                    mensagem = bex.BusinessError.msgErro,
                    origem = bex.BusinessError.origemErro
                };
            }
            else
            {
                response = new
                {
                    tipo = (int)EnumTipoErro.NEGOCIO,
                    codigo = bex.ErrorCode,
                    mensagem = bex.Message,
                    origem = "pix-pagador"
                };
            }

            var responseBytes = SerializeToJson(response);
            await context.Response.Body.WriteAsync(responseBytes);
        }

        private async Task WriteValidationExceptionDirectAsync(HttpContext context, ValidationException vex, string correlationId)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            var response = new
            {
                tipo = (int)EnumTipoErro.SISTEMA,
                codigo = 400,
                mensagem = vex.Message,
                origem = "pix-pagador"
            };

            var responseBytes = SerializeToJson(response);
            await context.Response.Body.WriteAsync(responseBytes);
        }

        private static byte[] SerializeToJson(object obj)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

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