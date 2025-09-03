using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using Domain.Core.Models.Transactions;
using Domain.Core.Ports.Domain;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;


namespace Adapters.Inbound.WebAPI.Endpoints
{
    public static partial class CDBEndpoint
    {
        public static void AddCDBEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("bks/v1/cdb/operacoes")
                         .WithTags("CD Operacoes")
                         .RequireAuthorization();

            // Health check endpoint - does not require authorization
            group.MapGet("health", () =>
            {
                var healthInfo = new
                {
                    status = "Healthy",
                    service = "CDB Operacoes API",
                    timestamp = DateTime.UtcNow
                };

                return Results.Ok(healthInfo);
            })
            .AllowAnonymous()
            .WithName("CDB Operations Health Check")
            .WithDescription("Endpoint de healthcheck para monitoramento do serviço de operações CDB")
            .Produces<object>(StatusCodes.Status200OK);


            group.MapPost("ListaPapDispAplic", async (
                  HttpContext httpContext,
                  [FromBody] RequestConsultaLista request,
                  [FromServices] BSMediator bSMediator,
                  [FromServices] ITransactionFactory transactionFactory,
                  [FromServices] CorrelationIdGenerator correlationIdGenerator
                  ) =>
            {
                var correlationId = correlationIdGenerator.GenerateWithPrefix("CDB");
                var transaction = transactionFactory.CreateConsultaPapelDispAplic(httpContext, request, correlationId);
                return await bSMediator.Send<TransactionConsultaPapelDispAplic, BaseReturn<ResponsePapelDispAplic>>(transaction);

            })
                  .WithName("Consulta Papel Disp Aplic")
                  .Produces<ResponsePapelDispAplic>(StatusCodes.Status200OK)
                  .Produces(StatusCodes.Status401Unauthorized)
                  .Produces(StatusCodes.Status400BadRequest);


            group.MapPost("AplicNoDia", async (
                HttpContext httpContext,
                [FromBody] RequestConsultaLista request,
                [FromServices] BSMediator bSMediator,
                [FromServices] ITransactionFactory transactionFactory,
                [FromServices] CorrelationIdGenerator correlationIdGenerator
                ) =>
            {
                var correlationId = correlationIdGenerator.GenerateWithPrefix("CDB");
                var transaction = transactionFactory.CreateConsultaAplicacaoDia(httpContext, request, correlationId);
                return await bSMediator.Send<TransactionConsultaAplicacaoDia, BaseReturn<ResponseAplicacaoNoDia>>(transaction);

            })
                .WithName("Consulta Aplicacoes No Dia")
                .Produces<ResponseAplicacaoNoDia>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status400BadRequest);


            group.MapPost("ListAplicDoCliPorTipPap", async (
                HttpContext httpContext,
                [FromBody] RequestConsultaLista request,
                [FromServices] BSMediator bSMediator,
                [FromServices] ITransactionFactory transactionFactory,
                [FromServices] CorrelationIdGenerator correlationIdGenerator
                ) =>
            {
                var correlationId = correlationIdGenerator.GenerateWithPrefix("CDB");
                var transaction = transactionFactory.CreateConsultarAplicacaoPorTipoPapel(httpContext, request, correlationId);
                return await bSMediator.Send<TransactionConsultarAplicacaoPorTipoPapel, BaseReturn<ResponseAplicacaoPorTipoPapel>>(transaction);

            })
                .WithName("Consulta Lista Aplicacoes do Cliente por Tipo de Papel")
                .Produces<ResponseAplicacaoPorTipoPapel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status400BadRequest);


            group.MapPost("ListSalTotAplicDoCliPorTipPap", async (
                HttpContext httpContext,
                [FromBody] RequestConsultaLista request,
                [FromServices] BSMediator bSMediator,
                [FromServices] ITransactionFactory transactionFactory,
                [FromServices] CorrelationIdGenerator correlationIdGenerator
                ) =>
                        {
                            var correlationId = correlationIdGenerator.GenerateWithPrefix("CDB");
                            var transaction = transactionFactory.CreateConsultaSaldoTotalPapel(httpContext, request, correlationId);
                            return await bSMediator.Send<TransactionConsultaSaldoTotalPapel, BaseReturn<ResponseSaldoPorTipoPapel>>(transaction);

                        })
                .WithName("Consulta Saldo Total Aplicacoes do Cliente por Tipo de Papel")
                .Produces<ResponseSaldoPorTipoPapel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status400BadRequest);


            group.MapPost("ListPapApliCli", async (
                  HttpContext httpContext,
                  [FromBody] RequestConsultaLista request,
                  [FromServices] BSMediator bSMediator,
                  [FromServices] ITransactionFactory transactionFactory,
                  [FromServices] CorrelationIdGenerator correlationIdGenerator
                  ) =>
                        {
                            var correlationId = correlationIdGenerator.GenerateWithPrefix("CDB");
                            var transaction = transactionFactory.CreateConsultaPapeisCarteira(httpContext, request, correlationId);
                            return await bSMediator.Send<TransactionConsultaPapeisCarteira, BaseReturn<ResponsePapeisPorCarteira>>(transaction);

                        })
                  .WithName("Consulta Papeis por tipo Carteira")
                  .Produces<ResponsePapeisPorCarteira>(StatusCodes.Status200OK)
                  .Produces(StatusCodes.Status401Unauthorized)
                  .Produces(StatusCodes.Status400BadRequest);


            group.MapPost("ListaCartApliCli", async (
                   HttpContext httpContext,
                   [FromBody] RequestConsultaLista request,
                   [FromServices] BSMediator bSMediator,
                   [FromServices] ITransactionFactory transactionFactory,
                   [FromServices] CorrelationIdGenerator correlationIdGenerator
                   ) =>
            {
                var correlationId = correlationIdGenerator.GenerateWithPrefix("CDB");
                var transaction = transactionFactory.CreateConsultaCarteiraAplicacao(httpContext, request, correlationId);
                return await bSMediator.Send<TransactionConsultaCarteiraAplicacao, BaseReturn<ResponseCarteira>>(transaction);

            })
                   .WithName("Consulta Carteiras por tipo Aplicacao")
                   .Produces<ResponseCarteira>(StatusCodes.Status200OK)
                   .Produces(StatusCodes.Status401Unauthorized)
                   .Produces(StatusCodes.Status400BadRequest);


            group.MapPost("ListaTipOpeCli", async (
              HttpContext httpContext,
              [FromBody] RequestConsultaLista request,
              [FromServices] BSMediator bSMediator,
              [FromServices] ITransactionFactory transactionFactory,
              [FromServices] CorrelationIdGenerator correlationIdGenerator
              ) =>
            {
                var correlationId = correlationIdGenerator.GenerateWithPrefix("CDB");
                var transaction = transactionFactory.CreateConsultaListaOperacoes(httpContext, request, correlationId);
                return await bSMediator.Send<TransactionConsultaListaOperacoes, BaseReturn<ResponseTipoOperacao>>(transaction);

            })
              .WithName("Consulta Lista Operacoes Cliente")
              .Produces<ResponseCarteira>(StatusCodes.Status200OK)
              .Produces(StatusCodes.Status401Unauthorized)
              .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
