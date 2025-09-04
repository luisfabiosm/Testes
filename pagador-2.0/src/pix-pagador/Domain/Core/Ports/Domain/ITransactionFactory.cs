using Domain.Core.Models.Request;
using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;

namespace Domain.Core.Ports.Domain
{
    public interface ITransactionFactory
    {
        TransactionRegistrarOrdemPagamento CreateRegistrarOrdemPagamento(
       HttpContext context, JDPIRegistrarOrdemPagtoRequest request, string correlationId);

        TransactionEfetivarOrdemPagamento CreateEfetivarOrdemPagamento(
            HttpContext context, JDPIEfetivarOrdemPagtoRequest request, string correlationId);

        TransactionCancelarOrdemPagamento CreateCancelarOrdemPagamento(
            HttpContext context, JDPICancelarRegistroOrdemPagtoRequest request, string correlationId);

        TransactionRegistrarOrdemDevolucao CreateRegistrarOrdemDevolucao(
            HttpContext context, JDPIRequisitarDevolucaoOrdemPagtoRequest request, string correlationId);
        TransactionCancelarOrdemDevolucao CreateCancelarRegistroOrdemDevolucao(
           HttpContext context, JDPICancelarRegistroOrdemdDevolucaoRequest request, string correlationId);

        TransactionEfetivarOrdemDevolucao CreateEfetivarOrdemDevolucao(
            HttpContext context, JDPIEfetivarOrdemDevolucaoRequest request, string correlationId);
    }

}
