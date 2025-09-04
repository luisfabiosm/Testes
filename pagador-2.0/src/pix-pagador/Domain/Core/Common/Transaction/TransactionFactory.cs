using Domain.Core.Models.Request;
using Domain.Core.Ports.Domain;
using Domain.Services;
using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;


namespace Domain.Core.Common.Transaction;

public class TransactionFactory : ITransactionFactory
{
    private readonly ContextAccessorService _contextAccessor;

    public TransactionFactory(ContextAccessorService contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public TransactionRegistrarOrdemPagamento CreateRegistrarOrdemPagamento(
        HttpContext context, JDPIRegistrarOrdemPagtoRequest request, string correlationId)
    {
        return new TransactionRegistrarOrdemPagamento
        {
            idReqSistemaCliente = request.idReqSistemaCliente,
            CorrelationId = correlationId,
            Code = 1,
            pagador = request.pagador,
            recebedor = request.recebedor,
            tpIniciacao = request.tpIniciacao,
            valor = request.valor,
            infEntreClientes = request.infEntreClientes,
            prioridadePagamento = request.prioridadePagamento,
            tpPrioridadePagamento = request.tpPrioridadePagamento,
            finalidade = request.finalidade,
            modalidadeAgente = request.modalidadeAgente,
            ispbPss = request.ispbPss,
            cnpjIniciadorPagamento = request.cnpjIniciadorPagamento,
            vlrDetalhe = request.vlrDetalhe,
            agendamentoID = request.agendamentoID,
            qrCode = request.qrCode,
            endToEndId = request.endToEndId,
            dtEnvioPag = request.dtEnvioPag,
            consentId = request.consentId,
            idConciliacaoRecebedor = request.idConciliacaoRecebedor,
            chave = request.chave,
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context)
        };
    }

    public TransactionEfetivarOrdemPagamento CreateEfetivarOrdemPagamento(
        HttpContext context, JDPIEfetivarOrdemPagtoRequest request, string correlationId)
    {
        return new TransactionEfetivarOrdemPagamento
        {
            idReqSistemaCliente = request.idReqSistemaCliente,
            CorrelationId = correlationId,
            Code = 2,
            agendamentoID = request.agendamentoID,
            idReqJdPi = request.idReqJdPi,
            endToEndId = request.endToEndId,
            dtHrReqJdPi = request.dtHrReqJdPi,
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context)

        };
    }

    public TransactionCancelarOrdemPagamento CreateCancelarOrdemPagamento(
        HttpContext context, JDPICancelarRegistroOrdemPagtoRequest request, string correlationId)
    {
        return new TransactionCancelarOrdemPagamento
        {
            idReqSistemaCliente = request.idReqSistemaCliente,
            CorrelationId = correlationId,
            Code = 3,
            agendamentoID = request.agendamentoID,
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context),
            motivo = request.motivo,
            tipoErro = request.tipoErro,
        };
    }


    public TransactionRegistrarOrdemDevolucao CreateRegistrarOrdemDevolucao(
       HttpContext context, JDPIRequisitarDevolucaoOrdemPagtoRequest request, string correlationId)
    {
        return new TransactionRegistrarOrdemDevolucao
        {
            CorrelationId = correlationId,
            Code = 4,
            idReqSistemaCliente = request.idReqSistemaCliente,
            endToEndIdOriginal = request.endToEndIdOriginal,
            endToEndIdDevolucao = request.endToEndIdDevolucao,
            codigoDevolucao = request.codigoDevolucao,
            motivoDevolucao = request.motivoDevolucao,
            valorDevolucao = request.valorDevolucao,
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context),
        };
    }

    public TransactionCancelarOrdemDevolucao CreateCancelarRegistroOrdemDevolucao(
       HttpContext context, JDPICancelarRegistroOrdemdDevolucaoRequest request, string correlationId)
    {
        return new TransactionCancelarOrdemDevolucao
        {
            CorrelationId = correlationId,
            Code = 5,
            idReqSistemaCliente = request.idReqSistemaCliente,
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context),
        };
    }

    public TransactionEfetivarOrdemDevolucao CreateEfetivarOrdemDevolucao(
      HttpContext context, JDPIEfetivarOrdemDevolucaoRequest request, string correlationId)
    {
        return new TransactionEfetivarOrdemDevolucao
        {
            CorrelationId = correlationId,
            Code = 6,
            idReqSistemaCliente = request.idReqSistemaCliente,
            canal = _contextAccessor.GetCanal(context),
            chaveIdempotencia = _contextAccessor.GetChaveIdempotencia(context),
            idReqJdPi = request.idReqJdPi,
            endToEndIdOriginal = request.endToEndIdOriginal,
            endToEndIdDevolucao = request.endToEndIdDevolucao,
            dtHrReqJdPi = request.dtHrReqJdPi,
        };
    }

}

