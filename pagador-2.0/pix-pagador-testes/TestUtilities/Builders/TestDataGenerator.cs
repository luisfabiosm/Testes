using AutoFixture;

using Domain.Core.Models.Request;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;

namespace pix_pagador_testes.TestUtilities.Builders;



public static class TestDataGenerator
{
    private static readonly Fixture _fixture = new();
    private static readonly Random _random = new();

    public static string GerarCpfValido()
    {
        // Gera um CPF válido para testes
        var cpf = new int[11];

        // Gera os 9 primeiros dígitos
        for (int i = 0; i < 9; i++)
        {
            cpf[i] = _random.Next(0, 10);
        }

        // Calcula o primeiro dígito verificador
        int soma = 0;
        for (int i = 0; i < 9; i++)
        {
            soma += cpf[i] * (10 - i);
        }
        int resto = soma % 11;
        cpf[9] = resto < 2 ? 0 : 11 - resto;

        // Calcula o segundo dígito verificador
        soma = 0;
        for (int i = 0; i < 10; i++)
        {
            soma += cpf[i] * (11 - i);
        }
        resto = soma % 11;
        cpf[10] = resto < 2 ? 0 : 11 - resto;

        return string.Join("", cpf);
    }

    public static string GerarCnpjValido()
    {
        // Gera um CNPJ válido para testes
        var cnpj = new int[14];

        // Gera os 12 primeiros dígitos
        for (int i = 0; i < 12; i++)
        {
            cnpj[i] = _random.Next(0, 10);
        }

        // Calcula o primeiro dígito verificador
        int[] peso1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int soma = 0;
        for (int i = 0; i < 12; i++)
        {
            soma += cnpj[i] * peso1[i];
        }
        int resto = soma % 11;
        cnpj[12] = resto < 2 ? 0 : 11 - resto;

        // Calcula o segundo dígito verificador
        int[] peso2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        soma = 0;
        for (int i = 0; i < 13; i++)
        {
            soma += cnpj[i] * peso2[i];
        }
        resto = soma % 11;
        cnpj[13] = resto < 2 ? 0 : 11 - resto;

        return string.Join("", cnpj);
    }

    public static string GerarEndToEndId()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var randomNumber = _random.Next(1000, 9999);
        return $"E12345678{timestamp}{randomNumber}";
    }

    public static string GerarChavePix()
    {
        var tipos = new[] { "email", "telefone", "cpf", "cnpj", "aleatoria" };
        var tipo = tipos[_random.Next(tipos.Length)];

        return tipo switch
        {
            "email" => $"usuario{_random.Next(1000, 9999)}@exemplo.com",
            "telefone" => $"+5511{_random.Next(10000000, 99999999)}",
            "cpf" => GerarCpfValido(),
            "cnpj" => GerarCnpjValido(),
            "aleatoria" => Guid.NewGuid().ToString(),
            _ => "usuario@exemplo.com"
        };
    }

    public static decimal GerarValorValido()
    {
        return Math.Round((decimal)(_random.NextDouble() * 10000 + 0.01), 2);
    }

    public static T GerarObjetoCompleto<T>() where T : class
    {
        return _fixture.Create<T>();
    }

    public static List<T> GerarLista<T>(int quantidade = 5) where T : class
    {
        return _fixture.CreateMany<T>(quantidade).ToList();
    }
}