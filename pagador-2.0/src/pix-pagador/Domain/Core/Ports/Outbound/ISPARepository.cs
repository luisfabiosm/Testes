using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;

namespace Domain.Core.Ports.Outbound
{
    public interface ISPARepository : IDisposable
    {


        ValueTask<string> RegistrarOrdemPagamento(TransactionRegistrarOrdemPagamento transaction);

        ValueTask<string> EfetivarOrdemPagamento(TransactionEfetivarOrdemPagamento transaction);

        ValueTask<string> CancelarOrdemPagamento(TransactionCancelarOrdemPagamento transaction);

        ValueTask<string> RegistrarOrdemDevolucao(TransactionRegistrarOrdemDevolucao transaction);

        ValueTask<string> EfetivarOrdemDevolucao(TransactionEfetivarOrdemDevolucao transaction);

        ValueTask<string> CancelarOrdemDevolucao(TransactionCancelarOrdemDevolucao transaction);

    }
}
