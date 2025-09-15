using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.UseCases.Devolucao
{
    public class TransactionCancelarOrdemDevolucaoBuilder
    {
        private TransactionCancelarOrdemDevolucao _transaction;

        public TransactionCancelarOrdemDevolucaoBuilder()
        {
            _transaction = new TransactionCancelarOrdemDevolucao
            {
                idReqSistemaCliente = "REQ123456789",
                CorrelationId = Guid.NewGuid().ToString(),
                Code = 5
            };
        }

        public TransactionCancelarOrdemDevolucaoBuilder ComIdReqSistemaCliente(string idReqSistemaCliente)
        {
            _transaction = _transaction with { idReqSistemaCliente = idReqSistemaCliente };
            return this;
        }

        public TransactionCancelarOrdemDevolucaoBuilder ComCorrelationId(string correlationId)
        {
            _transaction.CorrelationId = correlationId;
            return this;
        }

        public TransactionCancelarOrdemDevolucao Build() => _transaction;
    }

}
