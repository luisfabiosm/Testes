using Domain.Core.Models.Request;
using Domain.Core.Models.Transactions;
using Domain.Core.Ports.Domain;
using Domain.Services;


namespace Domain.Core.Common.Transaction;

public class TransactionFactory : ITransactionFactory
{
    private readonly ContextAccessorService _contextAccessor;

    public TransactionFactory(ContextAccessorService contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public TransactionConsultaPapelDispAplic CreateConsultaPapelDispAplic(
        HttpContext context, RequestConsultaLista request, string correlationId)
    {
        return new TransactionConsultaPapelDispAplic
        {
            CorrelationId = correlationId,
            Code = Convert.ToInt32(request.listaCarteira),
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context)
        };
    }

    public TransactionConsultaCarteiraAplicacao CreateConsultaCarteiraAplicacao(
       HttpContext context, RequestConsultaLista request, string correlationId)
    {
        return new TransactionConsultaCarteiraAplicacao
        {
            CorrelationId = correlationId,
            Code = Convert.ToInt32(request.listaCarteira),
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context)
        };
    }

    public TransactionConsultaListaOperacoes CreateConsultaListaOperacoes(
       HttpContext context, RequestConsultaLista request, string correlationId)
    {
        return new TransactionConsultaListaOperacoes
        {
            CorrelationId = correlationId,
            Code = Convert.ToInt32(request.listaCarteira),
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context)
        };
    }


    public TransactionConsultaPapeisCarteira CreateConsultaPapeisCarteira(
       HttpContext context, RequestConsultaLista request, string correlationId)
    {
        return new TransactionConsultaPapeisCarteira
        {
            CorrelationId = correlationId,
            Code = Convert.ToInt32(request.listaCarteira),
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context)
        };
    }

    public TransactionConsultaAplicacaoDia CreateConsultaAplicacaoDia(
      HttpContext context, RequestConsultaLista request, string correlationId)
    {
        return new TransactionConsultaAplicacaoDia
        {
            CorrelationId = correlationId,
            Code = Convert.ToInt32(request.listaCarteira),
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context)
        };
    }

    public TransactionConsultarAplicacaoPorTipoPapel CreateConsultarAplicacaoPorTipoPapel(
      HttpContext context, RequestConsultaLista request, string correlationId)
    {
        return new TransactionConsultarAplicacaoPorTipoPapel
        {
            CorrelationId = correlationId,
            Code = Convert.ToInt32(request.listaCarteira),
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context)
        };
    }

    public TransactionConsultaSaldoTotalPapel CreateConsultaSaldoTotalPapel(
      HttpContext context, RequestConsultaLista request, string correlationId)
    {
        return new TransactionConsultaSaldoTotalPapel
        {
            CorrelationId = correlationId,
            Code = Convert.ToInt32(request.listaCarteira),
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context)
        };
    }
}

