using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.UseCases.Pagamento
{
    public class TransactionEfetivarOrdemPagamentoBuilder
    {
        private TransactionEfetivarOrdemPagamento _transaction;

        public TransactionEfetivarOrdemPagamentoBuilder()
        {
            _transaction = new TransactionEfetivarOrdemPagamento
            {
                idReqSistemaCliente = "REQ123456789",
                CorrelationId = Guid.NewGuid().ToString(),
                Code = 2,
                canal = 26,
                chaveIdempotencia = Guid.NewGuid().ToString(),
                agendamentoID = "AGENDA123",
                idReqJdPi = "JDPI123456789",
                endToEndId = "E12345678901234567890123456789012",
                dtHrReqJdPi = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
            };
        }

        public TransactionEfetivarOrdemPagamentoBuilder ComIdReqSistemaCliente(string idReqSistemaCliente)
        {
            _transaction = _transaction with { idReqSistemaCliente = idReqSistemaCliente };
            return this;
        }

        public TransactionEfetivarOrdemPagamentoBuilder ComEndToEndId(string endToEndId)
        {
            _transaction = _transaction with { endToEndId = endToEndId };
            return this;
        }

        public TransactionEfetivarOrdemPagamentoBuilder ComIdReqJdPi(string idReqJdPi)
        {
            _transaction = _transaction with { idReqJdPi = idReqJdPi };
            return this;
        }

        public TransactionEfetivarOrdemPagamentoBuilder ComAgendamentoID(string agendamentoID)
        {
            _transaction = _transaction with { agendamentoID = agendamentoID };
            return this;
        }

        public TransactionEfetivarOrdemPagamentoBuilder ComDtHrReqJdPi(string dtHrReqJdPi)
        {
            _transaction = _transaction with { dtHrReqJdPi = dtHrReqJdPi };
            return this;
        }

        public TransactionEfetivarOrdemPagamentoBuilder ComCorrelationId(string correlationId)
        {
            _transaction.CorrelationId = correlationId;
            return this;
        }

        public TransactionEfetivarOrdemPagamento Build() => _transaction;
    }

}
