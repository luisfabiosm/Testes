using Domain.Core.Enum;
using Domain.Core.Models.JDPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.TestUtilities.Builders;

public static class RecebedorBuilder
{
    public static RecebedorEntityBuilder Create()
    {
        return new RecebedorEntityBuilder();
    }
}

public class RecebedorEntityBuilder
{
    private readonly JDPIDadosConta _recebedor;

    public RecebedorEntityBuilder()
    {
        _recebedor = new JDPIDadosConta
        {
            ispb = 123456,
            cpfCnpj = 63933527287,
            nome = "Fulano Recebedor",
            nrAgencia = "15",
            nrConta = "123456",
            tpConta = EnumTipoConta.CORRENTE,
            tpPessoa = EnumTipoPessoa.PESSOA_JURIDICA,
        };
    }

    public RecebedorEntityBuilder ComDadosValidos()
    {
        return this;
    }

    public RecebedorEntityBuilder ComCnpj(long cnpj)
    {
        _recebedor.cpfCnpj = cnpj;
        return this;
    }

    public RecebedorEntityBuilder ComNome(string nome)
    {
        _recebedor.nome = nome;
        return this;
    }

    public JDPIDadosConta Build()
    {
        return _recebedor;
    }
}
