using Domain.Core.Enum;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;


namespace pix_pagador_testes.Domain.UseCases.Pagamento
{
    public class TransactionCancelarOrdemPagamentoBuilder
    {
        private TransactionCancelarOrdemPagamento _transaction;

        public TransactionCancelarOrdemPagamentoBuilder()
        {
            _transaction = new TransactionCancelarOrdemPagamento
            {
                idReqSistemaCliente = "REQ123456789",
                CorrelationId = Guid.NewGuid().ToString(),
                Code = 3,
                canal = 26,
                chaveIdempotencia = Guid.NewGuid().ToString(),
                agendamentoID = "AGENDA123",
                motivo = "Cancelamento solicitado pelo cliente",
                tipoErro = EnumTipoErro.NEGOCIO
            };
        }

        public TransactionCancelarOrdemPagamentoBuilder ComIdReqSistemaCliente(string idReqSistemaCliente)
        {
            _transaction = _transaction with { idReqSistemaCliente = idReqSistemaCliente };
            return this;
        }

        public TransactionCancelarOrdemPagamentoBuilder ComMotivo(string motivo)
        {
            _transaction = _transaction with { motivo = motivo };
            return this;
        }

        public TransactionCancelarOrdemPagamentoBuilder ComTipoErro(EnumTipoErro tipoErro)
        {
            _transaction = _transaction with { tipoErro = tipoErro };
            return this;
        }

        public TransactionCancelarOrdemPagamentoBuilder ComAgendamentoID(string agendamentoID)
        {
            _transaction = _transaction with { agendamentoID = agendamentoID };
            return this;
        }

        public TransactionCancelarOrdemPagamentoBuilder ComCorrelationId(string correlationId)
        {
            _transaction.CorrelationId = correlationId;
            return this;
        }

        public TransactionCancelarOrdemPagamento Build() => _transaction;
    }

}
