using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Services;
using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Microsoft.AspNetCore.Mvc;


namespace Adapters.Inbound.WebApi.Pix.Endpoints
{
    public static partial class DevolucaoEndpoint
    {
        public static void AddDevolucaoEndpoints(this WebApplication app)
        {

            var group = app.MapGroup("soa/pix/api/v1/devolucao")
                         .WithTags("PIX Pagador")
                         .RequireAuthorization();


            group.MapPost("requisitar", async (
                  HttpContext httpContext,
                  [FromBody] JDPIRequisitarDevolucaoOrdemPagtoRequest request,
                  [FromServices] BSMediator bSMediator,
                  [FromServices] ITransactionFactory transactionFactory,
                  [FromServices] CorrelationIdGenerator correlationIdGenerator
                  ) =>
            {
                var correlationId = correlationIdGenerator.GenerateWithPrefix("DEV");
                var transaction = transactionFactory.CreateRegistrarOrdemDevolucao(httpContext, request, correlationId);
                return await bSMediator.Send<TransactionRegistrarOrdemDevolucao, BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>>(transaction);

            })
                  .WithName("Requisitar Ordem Devolução")
                  .WithDescription("Requisitar inicio de Ordem de Devolução")
                  .Produces<JDPIRegistrarOrdemDevolucaoResponse>(StatusCodes.Status200OK)
                  .Produces(StatusCodes.Status401Unauthorized)
                  .Produces(StatusCodes.Status400BadRequest);




            group.MapPost("cancelar", async (
                  HttpContext httpContext,
                  [FromBody] JDPICancelarRegistroOrdemdDevolucaoRequest request,
                  [FromServices] BSMediator bSMediator,
                  [FromServices] ITransactionFactory transactionFactory,
                  [FromServices] CorrelationIdGenerator correlationIdGenerator
                  ) =>
            {
                var correlationId = correlationIdGenerator.GenerateWithPrefix("CDEV");
                var transaction = transactionFactory.CreateCancelarRegistroOrdemDevolucao(httpContext, request, correlationId);
                return await bSMediator.Send<TransactionCancelarOrdemDevolucao, BaseReturn<JDPICancelarOrdemDevolucaoResponse>>(transaction);
            })
                  .WithName("Cancelar Ordem Devolução")
                  .WithDescription("Cancelar Ordem de Devolução iniciada")
                  .Produces<JDPICancelarOrdemDevolucaoResponse>(StatusCodes.Status200OK)
                  .Produces(StatusCodes.Status401Unauthorized)
                  .Produces(StatusCodes.Status400BadRequest);




            group.MapPost("efetivar", async (
                  HttpContext httpContext,
                  [FromBody] JDPIEfetivarOrdemDevolucaoRequest request,
                  [FromServices] BSMediator bSMediator,
                  [FromServices] ITransactionFactory transactionFactory,
                  [FromServices] CorrelationIdGenerator correlationIdGenerator
                 ) =>
            {
                var correlationId = correlationIdGenerator.GenerateWithPrefix("EDEV");
                var transaction = transactionFactory.CreateEfetivarOrdemDevolucao(httpContext, request, correlationId);
                return await bSMediator.Send<TransactionEfetivarOrdemDevolucao, BaseReturn<JDPIEfetivarOrdemDevolucaoResponse>>(transaction);

            })
               .WithName("Efetivar Ordem Devolução")
               .WithDescription("Efetivar Ordem de Devolução registrada")
               .Produces<JDPIEfetivarOrdemDevolucaoResponse>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status401Unauthorized)
               .Produces(StatusCodes.Status400BadRequest);
        }

    }
}
