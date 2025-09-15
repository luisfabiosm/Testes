using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.UseCases.Devolucao
{

    public class TransactionRegistrarOrdemDevolucaoBuilder
    {
        private TransactionRegistrarOrdemDevolucao _transaction;

        public TransactionRegistrarOrdemDevolucaoBuilder()
        {
            _transaction = new TransactionRegistrarOrdemDevolucao
            {
                idReqSistemaCliente = "REQ123456789",
                endToEndIdOriginal = "E12345678901234567890123456789012",
                endToEndIdDevolucao = "D12345678901234567890123456789012",
                codigoDevolucao = "CD001",
                motivoDevolucao = "Motivo de teste",
                valorDevolucao = 100.50,
                chaveIdempotencia = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Code = 4
            };
        }

        public TransactionRegistrarOrdemDevolucaoBuilder ComIdReqSistemaCliente(string idReqSistemaCliente)
        {
            _transaction = _transaction with { idReqSistemaCliente = idReqSistemaCliente };
            return this;
        }

        public TransactionRegistrarOrdemDevolucaoBuilder ComEndToEndIdOriginal(string endToEndIdOriginal)
        {
            _transaction = _transaction with { endToEndIdOriginal = endToEndIdOriginal };
            return this;
        }

        public TransactionRegistrarOrdemDevolucaoBuilder ComCodigoDevolucao(string codigoDevolucao)
        {
            _transaction = _transaction with { codigoDevolucao = codigoDevolucao };
            return this;
        }

        public TransactionRegistrarOrdemDevolucaoBuilder ComValorDevolucao(double valorDevolucao)
        {
            _transaction = _transaction with { valorDevolucao = valorDevolucao };
            return this;
        }

        public TransactionRegistrarOrdemDevolucao Build() => _transaction;
    }

}
