using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Services;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using Microsoft.AspNetCore.Mvc;



namespace Adapters.Inbound.WebApi.Pix.Endpoints
{
    public static partial class OrdemPagamentoEndpoints
    {


        public static void AddOrdemPagamentoEndpoints(this WebApplication app)
        {

            var group = app.MapGroup("soa/pix/api/v1/debito")
                         .WithTags("PIX Pagador")
                         .RequireAuthorization();

            _ = group.MapPost("registrar", async (
                    HttpContext httpContext,
                    [FromBody] JDPIRegistrarOrdemPagtoRequest request,
                    [FromServices] BSMediator bSMediator,
                    [FromServices] ITransactionFactory transactionFactory,
                    [FromServices] CorrelationIdGenerator correlationIdGenerator // ✅ NOVO PARÂMETRO
                    ) =>
                {

                    var correlationId = correlationIdGenerator.GenerateWithPrefix("REG");
                    var transaction = transactionFactory.CreateRegistrarOrdemPagamento(httpContext, request, correlationId);
                    return await bSMediator.Send<TransactionRegistrarOrdemPagamento, BaseReturn<JDPIRegistrarOrdemPagamentoResponse>>(transaction);

                })
                .WithName("Registrar Ordem Pagamento")
                .WithDescription("Iniciar registrar de Ordem de Pagamento")
                .Produces<JDPIRegistrarOrdemPagamentoResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status400BadRequest);



            group.MapPost("cancelar", async (
                    HttpContext httpContext,
                    [FromBody] JDPICancelarRegistroOrdemPagtoRequest request,
                    [FromServices] BSMediator bSMediator,
                    [FromServices] ITransactionFactory transactionFactory,
                    [FromServices] CorrelationIdGenerator correlationIdGenerator
                    ) =>
                {
                    var correlationId = correlationIdGenerator.GenerateWithPrefix("CAN");
                    var transaction = transactionFactory.CreateCancelarOrdemPagamento(httpContext, request, correlationId);
                    return await bSMediator.Send<TransactionCancelarOrdemPagamento, BaseReturn<JDPICancelarOrdemPagamentoResponse>>(transaction);

                })
                .WithName("Cancelar Ordem Pagamento")
                .WithDescription("Cancelar Ordem de Pagamento registrada")
                .Produces<JDPICancelarOrdemPagamentoResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status400BadRequest);



            group.MapPost("efetivar", async (
                   HttpContext httpContext,
                   [FromBody] JDPIEfetivarOrdemPagtoRequest request,
                   [FromServices] BSMediator bSMediator,
                   [FromServices] ITransactionFactory transactionFactory,
                   [FromServices] CorrelationIdGenerator correlationIdGenerator
                   ) =>
             {
                 var correlationId = correlationIdGenerator.GenerateWithPrefix("EFE");
                 var transaction = transactionFactory.CreateEfetivarOrdemPagamento(httpContext, request, correlationId);
                 return await bSMediator.Send<TransactionEfetivarOrdemPagamento, BaseReturn<JDPIEfetivarOrdemPagamentoResponse>>(transaction);
             })
             .WithName("Efetivar Ordem Pagamento")
             .WithDescription("Efetivar Ordem de Pagamento registrada")
             .Produces<JDPIEfetivarOrdemPagamentoResponse>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
              .Produces(StatusCodes.Status400BadRequest);
        }
    }
}

