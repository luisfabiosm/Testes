using Domain.Core.Models.Request;
using Domain.Core.Models.Transactions;


namespace Domain.Core.Ports.Domain
{
    public interface ITransactionFactory
    {
        TransactionConsultaPapelDispAplic CreateConsultaPapelDispAplic(
         HttpContext context, RequestConsultaLista request, string correlationId);

        TransactionConsultaCarteiraAplicacao CreateConsultaCarteiraAplicacao(
         HttpContext context, RequestConsultaLista request, string correlationId);

        TransactionConsultaListaOperacoes CreateConsultaListaOperacoes(
         HttpContext context, RequestConsultaLista request, string correlationId);

        TransactionConsultaPapeisCarteira CreateConsultaPapeisCarteira(
         HttpContext context, RequestConsultaLista request, string correlationId);

        TransactionConsultaAplicacaoDia CreateConsultaAplicacaoDia(
         HttpContext context, RequestConsultaLista request, string correlationId);

        TransactionConsultarAplicacaoPorTipoPapel CreateConsultarAplicacaoPorTipoPapel(
         HttpContext context, RequestConsultaLista request, string correlationId);

        TransactionConsultaSaldoTotalPapel CreateConsultaSaldoTotalPapel(
         HttpContext context, RequestConsultaLista request, string correlationId);
    }
}
