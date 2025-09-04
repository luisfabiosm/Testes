using Microsoft.AspNetCore.Mvc;
using Domain.Core.Models.SPA;
using Domain.Core.Enums;
using System.Text.Json;
using System.Net;

namespace Domain.Core.Base
{
    public struct BaseReturn : IDisposable
    {
        public object? Body;

        public HttpStatusCode StatusCode = HttpStatusCode.OK;
        public string? DataResponse { get; set; }
        public string? Mensagem { get; set; }
        public EnumStatus Status { get; set; }
        public SPAError? InternalError { get; internal set; }
        public BaseReturn() { }

        public BaseReturn(string data, string mensagem, EnumStatus status, dynamic? internalerror = null)
        {
            DataResponse = data;
            Mensagem = mensagem;
            Status = status;
            InternalError = internalerror;
        }

        public BaseReturn(object result, EnumReturnType type = EnumReturnType.SUCCESS)
        {
            StatusCode = type switch
            {
                EnumReturnType.SYSTEM => HttpStatusCode.InternalServerError,
                EnumReturnType.BUSINESS => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.OK
            };

            if (result is not null && result is Exception messageResult)
            {
                InternalError =  new SPAError
                {
                    Mensagem = messageResult.Message,
                    Tipo = EnumSPATipoErroInterno.Geral,
                    Codigo = -1
                };
            }

            Body = result is not Exception ? result : null;
        }

        public IActionResult RetornoOK(object retorno)
        {
            return new OkObjectResult(retorno.ToString());
        }

        public BaseReturn RetornoOk(object? retorno = null)
        {
            return new BaseReturn
            {
                DataResponse = DateTime.UtcNow.ToString(),
                Mensagem = JsonSerializer.Serialize(retorno ?? ""),
                Status = EnumStatus.SUCESSO,
                InternalError = null
            };
        }

        public IResult RetornoAccepted(object? retorno = null)
        {
            return retorno != null
                ? Results.Accepted(string.Empty, retorno)
                : Results.Accepted();
        }

        public IResult RetornoERRO()
        {
            var statusCode = (int)StatusCode;

            return Results.Problem(
                detail: InternalError?.Mensagem ?? "Erro interno",
                statusCode: statusCode,
                title: "Erro",
                type: InternalError?.Tipo.ToString());
        }

        public BaseReturn RetornoErro(Exception ex)
        {
            return new BaseReturn
            {
                DataResponse = DateTime.UtcNow.ToString(),
                Mensagem = ex.Message,
                Status = EnumStatus.SISTEMA,
                InternalError = new SPAError(ex)
            };
        }

        public void Dispose()
        {
            DataResponse = null;
            Mensagem = null;
            InternalError = null;
        }
    }
}
