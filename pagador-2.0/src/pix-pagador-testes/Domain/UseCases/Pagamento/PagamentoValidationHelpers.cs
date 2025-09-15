using Domain.Core.Common.ResultPattern;
using Domain.Core.Enum;
using Domain.Core.Exceptions;
using Domain.Core.Models.JDPI;

namespace pix_pagador_testes.Domain.UseCases.Pagamento;

public static class PagamentoValidationHelpers
{
    public static ValidationResult CreateInvalid(params ErrorDetails[] errors)
    {
        var errorList = errors?.ToList() ?? new List<ErrorDetails>();

        try
        {
            return ValidationResult.Invalid(errorList);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Não foi possível criar ValidationResult inválido. " +
                $"Verifique a implementação da classe ValidationResult. " +
                $"Erro: {ex.Message}", ex);
        }
    }

    public static ValidationResult CreateValid()
    {
        try
        {
            return ValidationResult.Valid();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Não foi possível criar ValidationResult válido. " +
                $"Verifique a implementação da classe ValidationResult. " +
                $"Erro: {ex.Message}", ex);
        }
    }

    public static void VerifyValidationResult(ValidationResult result, bool shouldBeValid, int expectedErrorCount = 0)
    {
        Assert.NotNull(result);
        Assert.Equal(shouldBeValid, result.IsValid);

        if (shouldBeValid)
        {
            Assert.True(result.Errors == null || !result.Errors.Any());
        }
        else
        {
            Assert.NotNull(result.Errors);
            Assert.Equal(expectedErrorCount, result.Errors.Count);
        }
    }

    public static void VerifyErrorContainsField(ValidationResult result, string fieldName)
    {
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.campo == fieldName);
    }

    public static void VerifyErrorContainsFieldAndMessage(ValidationResult result, string fieldName, string expectedMessage)
    {
        Assert.False(result.IsValid);
        var error = result.Errors.FirstOrDefault(e => e.campo == fieldName);
        Assert.NotNull(error);
        Assert.Equal(expectedMessage, error.mensagens);
    }
}

public static class PagamentoTestDataFactory
{
    public static JDPIDadosConta CreateValidPagadorPessoaFisica()
    {
        return new JDPIDadosConta
        {
            ispb = 12345678,
            cpfCnpj = 12345678901,
            nome = "João Silva Santos",
            tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
            tpConta = EnumTipoConta.CORRENTE,
            nrAgencia = "1234",
            nrConta = "567890123"
        };
    }

    public static JDPIDadosConta CreateValidPagadorPessoaJuridica()
    {
        return new JDPIDadosConta
        {
            ispb = 12345678,
            cpfCnpj = 12345678000195,
            nome = "Empresa Teste Ltda",
            tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
            tpConta = EnumTipoConta.CORRENTE,
            nrAgencia = "1234",
            nrConta = "567890123"
        };
    }

    public static JDPIDadosConta CreateValidRecebedorPessoaFisica()
    {
        return new JDPIDadosConta
        {
            ispb = 87654321,
            cpfCnpj = 98765432100,
            nome = "Maria Santos Silva",
            tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
            tpConta = EnumTipoConta.POUPANCA,
            nrAgencia = "4321",
            nrConta = "098765432"
        };
    }

    public static JDPIDadosConta CreateValidRecebedorPessoaJuridica()
    {
        return new JDPIDadosConta
        {
            ispb = 87654321,
            cpfCnpj = 98765432000189,
            nome = "Recebedor Empresarial S.A.",
            tpPessoa = EnumTipoPessoa.PESSOA_JURIDICA,
            tpConta = EnumTipoConta.CORRENTE,
            nrAgencia = "4321",
            nrConta = "098765432"
        };
    }

    public static JDPIDadosConta CreateInvalidPagador(string invalidField)
    {
        var pagador = CreateValidPagadorPessoaFisica();

        switch (invalidField.ToLower())
        {
            case "ispb":
                pagador.ispb = 0;
                break;
            case "cpfcnpj":
                pagador.cpfCnpj = 0;
                break;
            case "nome":
                pagador.nome = "";
                break;
            case "agencia":
                pagador.nrAgencia = "";
                break;
            case "conta":
                pagador.nrConta = "";
                break;
            default:
                throw new ArgumentException($"Campo inválido: {invalidField}");
        }

        return pagador;
    }

    public static JDPIDadosConta CreateInvalidRecebedor(string invalidField)
    {
        var recebedor = CreateValidRecebedorPessoaFisica();

        switch (invalidField.ToLower())
        {
            case "ispb":
                recebedor.ispb = 0;
                break;
            case "cpfcnpj":
                recebedor.cpfCnpj = 0;
                break;
            case "nome":
                recebedor.nome = "";
                break;
            case "agencia":
                recebedor.nrAgencia = "";
                break;
            case "conta":
                recebedor.nrConta = "";
                break;
            default:
                throw new ArgumentException($"Campo inválido: {invalidField}");
        }

        return recebedor;
    }

    public static List<JDPIValorDetalhe> CreateValidValorDetalhe()
    {
        return new List<JDPIValorDetalhe>
        {
            new JDPIValorDetalhe
            {
                tipo = EnumTipoDetalhe.VALOR_DA_COMPRA,
                vlrTarifaDinheiroCompra = 5
            }
        };
    }

    public static string[] GetValidEndToEndIds()
    {
        return new[]
        {
            "E12345678901234567890123456789012",
            "E98765432109876543210987654321098",
            "E11111111111111111111111111111111",
            "E99999999999999999999999999999999",
            "E00000000000000000000000000000000"
        };
    }

    public static string[] GetInvalidEndToEndIds()
    {
        return new[]
        {
            "",
            "E123", // Muito curto
            "E123456789012345678901234567890123", // Muito longo
            "X12345678901234567890123456789012", // Não começa com E
            "e12345678901234567890123456789012", // Minúsculo
            null
        };
    }

    public static double[] GetValidValues()
    {
        return new[]
        {
            0.01,
            1.00,
            100.50,
            999.99,
            1000.00,
            9999.99,
            99999.99
        };
    }

    public static double[] GetInvalidValues()
    {
        return new[]
        {
            0.00,
            -0.01,
            -100.00,
            -1.00
        };
    }

    public static string[] GetValidIdReqSistemaCliente()
    {
        return new[]
        {
            "REQ123456789",
            "REQ_TEST_001",
            "CLIENT_REQ_999",
            "SYS_REQ_ABC123"
        };
    }

    public static string[] GetInvalidIdReqSistemaCliente()
    {
        return new[]
        {
            "",
            null,
            "   ", // Apenas espaços
            "A", // Muito curto (se houver regra de tamanho mínimo)
        };
    }

    public static EnumTpIniciacao[] GetAllTpIniciacao()
    {
        return new[]
        {
            EnumTpIniciacao.QR_CODE_ESTATICO,
            EnumTpIniciacao.QR_CODE_DINAMICO,
            EnumTpIniciacao.QR_CODE_PAGADOR
        };
    }

    public static EnumTipoErro[] GetAllTipoErro()
    {
        return new[]
        {
            EnumTipoErro.SISTEMA,
            EnumTipoErro.NEGOCIO
        };
    }

    public static string[] GetValidMotivos()
    {
        return new[]
        {
            "Cancelamento solicitado pelo cliente",
            "Erro no processamento",
            "Solicitação de estorno",
            "Falha na comunicação",
            "Problema técnico identificado"
        };
    }

    public static string[] GetInvalidMotivos()
    {
        return new[]
        {
            "",
            null,
            "   " // Apenas espaços
        };
    }
}