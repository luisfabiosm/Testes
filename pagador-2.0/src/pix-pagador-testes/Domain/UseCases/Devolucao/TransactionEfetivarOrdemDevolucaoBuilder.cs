using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.UseCases.Devolucao
{
    public class TransactionEfetivarOrdemDevolucaoBuilder
    {
        private TransactionEfetivarOrdemDevolucao _transaction;

        public TransactionEfetivarOrdemDevolucaoBuilder()
        {
            _transaction = new TransactionEfetivarOrdemDevolucao
            {
                idReqSistemaCliente = "REQ123456789",
                idReqJdPi = "JDPI123456789",
                endToEndIdOriginal = "E12345678901234567890123456789012",
                endToEndIdDevolucao = "D12345678901234567890123456789012",
                dtHrReqJdPi = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                CorrelationId = Guid.NewGuid().ToString(),
                Code = 6
            };
        }

        public TransactionEfetivarOrdemDevolucaoBuilder ComIdReqSistemaCliente(string idReqSistemaCliente)
        {
            _transaction = _transaction with { idReqSistemaCliente = idReqSistemaCliente };
            return this;
        }

        public TransactionEfetivarOrdemDevolucaoBuilder ComEndToEndIdOriginal(string endToEndIdOriginal)
        {
            _transaction = _transaction with { endToEndIdOriginal = endToEndIdOriginal };
            return this;
        }

        public TransactionEfetivarOrdemDevolucaoBuilder ComIdReqJdPi(string idReqJdPi)
        {
            _transaction = _transaction with { idReqJdPi = idReqJdPi };
            return this;
        }

        public TransactionEfetivarOrdemDevolucao Build() => _transaction;
    }

}
