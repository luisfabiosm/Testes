using Domain.Core.Enum;
using Domain.Core.Models.JDPI;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;


namespace pix_pagador_testes.TestUtilities.Builders;


public static class TransactionBuilderOld
{
    private static readonly Fixture _fixture = new();

    public static TransactionRegistrarOrdemPagamentoBuilderOld CreateRegistrarOrdemPagamentoOld()
    {
        return new TransactionRegistrarOrdemPagamentoBuilderOld();
    }
}

public class TransactionRegistrarOrdemPagamentoBuilderOld
{
    private  TransactionRegistrarOrdemPagamento _transaction;
    private static readonly Fixture _fixture = new();

    public TransactionRegistrarOrdemPagamentoBuilderOld()
    {
        _transaction = new TransactionRegistrarOrdemPagamento
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString(),
            Code = 1,
            valor = 100.50,
            chave = "user@example.com",
            tpIniciacao = EnumTpIniciacao.CHAVE,
            prioridadePagamento =EnumPrioridadePagamento.LIQUIDACAO_PRIORITARIA,
            canal = 26,
            chaveIdempotencia = "IDEM123",
            pagador = new JDPIDadosConta
            {
                ispb = 4913711,
                cpfCnpj = 84759496220,
                nome = "JOSE SOARES",
                nrAgencia = "0013",
                nrConta = "2911043",
                tpConta = EnumTipoConta.CORRENTE,
                tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
            },

            recebedor = new JDPIDadosConta
            {
                ispb = 123456,
                cpfCnpj = 63933527287,
                nome = "Fulano Recebedor",
                nrAgencia = "15",
                nrConta = "123456",
                tpConta = EnumTipoConta.CORRENTE,
                tpPessoa = EnumTipoPessoa.PESSOA_JURIDICA,
            }
        };
    }

    public TransactionRegistrarOrdemPagamentoBuilderOld ComDadosValidos()
    {
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilderOld ComDadosInvalidos()
    {
        _transaction.pagador.nrAgencia = null;
        _transaction.pagador.nrConta = null;

        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilderOld ComCpfPagadorInvalido()
    {
        if (_transaction.pagador != null)
        {
            _transaction.pagador.cpfCnpj = 123123123;
        }
        return this;
    }

    public TransactionRegistrarOrdemPagamento Build()
    {
        return _transaction;
    }
}

