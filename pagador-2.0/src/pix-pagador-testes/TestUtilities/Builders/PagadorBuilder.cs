using Domain.Core.Enum;
using Domain.Core.Models.JDPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.TestUtilities.Builders;

public static class PagadorBuilder
{
    public static PagadorEntityBuilder Create()
    {
        return new PagadorEntityBuilder();
    }
}

public class PagadorEntityBuilder
{
    private readonly JDPIDadosConta _pagador;

    public PagadorEntityBuilder()
    {
        _pagador = new JDPIDadosConta
        {
            ispb = 4913711,
            cpfCnpj = 84759496220,
            nome = "JOSE SOARES",
            nrAgencia = "0013",
            nrConta = "2911043",
            tpConta = EnumTipoConta.CORRENTE,
            tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
        };
    }

    public PagadorEntityBuilder ComDadosValidos()
    {
        return this;
    }

    public PagadorEntityBuilder ComCpf(long cpf)
    {
        _pagador.cpfCnpj = cpf;
        return this;
    }

    public PagadorEntityBuilder ComNome(string nome)
    {
        _pagador.nome = nome;
        return this;
    }

    public JDPIDadosConta Build()
    {
        return _pagador;
    }
}