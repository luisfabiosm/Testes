using Domain.Core.Enum;
using Domain.Core.Models.JDPI;
using Domain.Core.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.TestUtilities.Builders;


public static class RequestBuilder
{
    private static readonly Fixture _fixture = new();

    public static JDPIRegistrarOrdemPagtoRequestBuilder CreateJDPIRegistrarOrdemPagtoRequest()
    {
        return new JDPIRegistrarOrdemPagtoRequestBuilder();
    }

    public static JDPIEfetivarOrdemPagtoRequestBuilder CreateJDPIEfetivarOrdemPagtoRequest()
    {
        return new JDPIEfetivarOrdemPagtoRequestBuilder();
    }

    public static JDPICancelarRegistroOrdemPagtoRequestBuilder CreateJDPICancelarRegistroOrdemPagtoRequest()
    {
        return new JDPICancelarRegistroOrdemPagtoRequestBuilder();
    }

    public static JDPIRequisitarDevolucaoOrdemPagtoRequestBuilder CreateJDPIRequisitarDevolucaoOrdemPagtoRequest()
    {
        return new JDPIRequisitarDevolucaoOrdemPagtoRequestBuilder();
    }
}

public class JDPIRegistrarOrdemPagtoRequestBuilder
{
    private readonly JDPIRegistrarOrdemPagtoRequest _request;

    public JDPIRegistrarOrdemPagtoRequestBuilder()
    {
        _request = new JDPIRegistrarOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ123456789",
            valor = 100.50,
            chave = "user@example.com",
            tpIniciacao = EnumTpIniciacao.CHAVE,
            prioridadePagamento = EnumPrioridadePagamento.LIQUIDACAO_PRIORITARIA,
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

    public JDPIRegistrarOrdemPagtoRequestBuilder ComDadosValidos()
    {
        return this;
    }

    public JDPIRegistrarOrdemPagtoRequestBuilder ComDadosInvalidos()
    {
        _request.pagador.cpfCnpj = 0;
        _request.valor = -100;
        return this;
    }

    public JDPIRegistrarOrdemPagtoRequestBuilder ComIdReqSistemaCliente(string id)
    {
        _request.idReqSistemaCliente = id;
        return this;
    }

    public JDPIRegistrarOrdemPagtoRequestBuilder ComValor(double valor)
    {
        _request.valor = valor;
        return this;
    }

    public JDPIRegistrarOrdemPagtoRequestBuilder ComChave(string chave)
    {
        _request.chave = chave;
        return this;
    }

    public JDPIRegistrarOrdemPagtoRequestBuilder ComDadosPagador(long cpf, string nome)
    {
        _request.pagador.cpfCnpj = cpf;
        _request.pagador.nome = nome;
        return this;
    }

    public JDPIRegistrarOrdemPagtoRequestBuilder ComDadosRecebedor(long cpf, string nome)
    {
        _request.recebedor.cpfCnpj = cpf;
        _request.recebedor.nome = nome;
        return this;
    }

    public JDPIRegistrarOrdemPagtoRequest Build()
    {
        return _request;
    }
}

public class JDPIEfetivarOrdemPagtoRequestBuilder
{
    private readonly JDPIEfetivarOrdemPagtoRequest _request;

    public JDPIEfetivarOrdemPagtoRequestBuilder()
    {
        _request = new JDPIEfetivarOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ123456789",
            agendamentoID = Guid.NewGuid().ToString(),
            idReqJdPi = "JDPI123456",
            endToEndId = "E12345678202412041200202412040001",
            dtHrReqJdPi = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
        };
    }

    public JDPIEfetivarOrdemPagtoRequestBuilder ComDadosValidos()
    {
        return this;
    }

    public JDPIEfetivarOrdemPagtoRequestBuilder ComEndToEndIdInexistente()
    {
        _request.endToEndId = "E99999999999999999999999999999999";
        return this;
    }

    public JDPIEfetivarOrdemPagtoRequestBuilder ComDataHora(DateTime dataHora)
    {
        _request.dtHrReqJdPi = dataHora.ToString("yyyy-MM-ddTHH:mm:ss");
        return this;
    }

    public JDPIEfetivarOrdemPagtoRequest Build()
    {
        return _request;
    }
}

public class JDPICancelarRegistroOrdemPagtoRequestBuilder
{
    private readonly JDPICancelarRegistroOrdemPagtoRequest _request;

    public JDPICancelarRegistroOrdemPagtoRequestBuilder()
    {
        _request = new JDPICancelarRegistroOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ123456789",
            agendamentoID = Guid.NewGuid().ToString(),
            motivo = "Solicitação do cliente",
            tipoErro = EnumTipoErro.NEGOCIO
        };
    }

    public JDPICancelarRegistroOrdemPagtoRequestBuilder ComDadosValidos()
    {
        return this;
    }

    public JDPICancelarRegistroOrdemPagtoRequestBuilder ComMotivoInvalido(string motivo)
    {
        _request.motivo = motivo;
        return this;
    }

    public JDPICancelarRegistroOrdemPagtoRequestBuilder ComMotivo(string motivo)
    {
        _request.motivo = motivo;
        return this;
    }

    public JDPICancelarRegistroOrdemPagtoRequestBuilder ComTipoErro(EnumTipoErro tipoErro)
    {
        _request.tipoErro = tipoErro;
        return this;
    }

    public JDPICancelarRegistroOrdemPagtoRequest Build()
    {
        return _request;
    }
}

public class JDPIRequisitarDevolucaoOrdemPagtoRequestBuilder
{
    private readonly JDPIRequisitarDevolucaoOrdemPagtoRequest _request;

    public JDPIRequisitarDevolucaoOrdemPagtoRequestBuilder()
    {
        _request = new JDPIRequisitarDevolucaoOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ123456789",
            valorDevolucao = 100.50,
            motivoDevolucao = "Devolução solicitada pelo cliente",
        };
    }

    public JDPIRequisitarDevolucaoOrdemPagtoRequestBuilder ComDadosValidos()
    {
        return this;
    }

    public JDPIRequisitarDevolucaoOrdemPagtoRequest Build()
    {
        return _request;
    }
}