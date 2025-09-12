using AutoFixture;
using Domain.Core.Enum;
using Domain.Core.Models.JDPI;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;

namespace pix_pagador_testes.TestUtilities.Builders;

public static class TransactionBuilder
{
    private static readonly Fixture _fixture = new();

    #region Pagamento Builders

    public static TransactionRegistrarOrdemPagamentoBuilder CreateRegistrarOrdemPagamentoPagadorNull()
    {
        return new TransactionRegistrarOrdemPagamentoBuilder(true);
    }

    public static TransactionRegistrarOrdemPagamentoBuilder CreateRegistrarOrdemPagamento()
    {
        return new TransactionRegistrarOrdemPagamentoBuilder();
    }

    public static TransactionEfetivarOrdemPagamentoBuilder CreateEfetivarOrdemPagamento()
    {
        return new TransactionEfetivarOrdemPagamentoBuilder();
    }

    public static TransactionCancelarOrdemPagamentoBuilder CreateCancelarOrdemPagamento()
    {
        return new TransactionCancelarOrdemPagamentoBuilder();
    }

    #endregion

    #region Devolução Builders

    public static TransactionRegistrarOrdemDevolucaoBuilder CreateRegistrarOrdemDevolucao()
    {
        return new TransactionRegistrarOrdemDevolucaoBuilder();
    }

    public static TransactionEfetivarOrdemDevolucaoBuilder CreateEfetivarOrdemDevolucao()
    {
        return new TransactionEfetivarOrdemDevolucaoBuilder();
    }

    public static TransactionCancelarOrdemDevolucaoBuilder CreateCancelarOrdemDevolucao()
    {
        return new TransactionCancelarOrdemDevolucaoBuilder();
    }

    #endregion

    #region Helper Methods


    public static JDPIDadosConta CreateNullJDPIDadosConta()
    {
        return null;
    }
    public static JDPIDadosConta CreateValidPagador()
    {
        return new JDPIDadosConta
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

    public static JDPIDadosConta CreateValidRecebedor()
    {
        return new JDPIDadosConta
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

    public static List<JDPIValorDetalhe> CreateValidValorDetalhe()
    {
        return new List<JDPIValorDetalhe>
        {
            new JDPIValorDetalhe
            {
                vlrTarifaDinheiroCompra = 1.50m,
                tipo = EnumTipoDetalhe.VALOR_DA_COMPRA
            }
        };
    }

    #endregion
}

#region Pagamento Builders

public class TransactionRegistrarOrdemPagamentoBuilder
{
    private TransactionRegistrarOrdemPagamento _transaction;
    private static readonly Fixture _fixture = new();

    public TransactionRegistrarOrdemPagamentoBuilder(bool nullFields = false)
    {
        if (nullFields)
        {
            _transaction = new TransactionRegistrarOrdemPagamento
            {
                idReqSistemaCliente = null,
                CorrelationId = null,
                Code = 1,
                valor = 100.50,
                chave = null,
                tpIniciacao = EnumTpIniciacao.CHAVE,
                canal = 26,
                chaveIdempotencia = null,
                pagador = TransactionBuilder.CreateNullJDPIDadosConta(),
                recebedor = TransactionBuilder.CreateNullJDPIDadosConta()
            };
            return;
        }
        _transaction = new TransactionRegistrarOrdemPagamento
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString(),
            Code = 1,
            valor = 100.50,
            chave = "user@example.com",
            tpIniciacao = EnumTpIniciacao.CHAVE,
            canal = 26,
            chaveIdempotencia = "IDEM123",
            pagador = TransactionBuilder.CreateValidPagador(),
            recebedor = TransactionBuilder.CreateValidRecebedor()

        };
       
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComDadosValidos()
    {
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComDadosInvalidos()
    {
        _transaction.pagador.nrAgencia = null;
        _transaction.pagador.nrConta = null;
        _transaction.recebedor.nome = null;
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComPagador(JDPIDadosConta pagador)
    {
        _transaction = _transaction with { pagador = pagador };
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComRecebedor(JDPIDadosConta recebedor)
    {
        _transaction = _transaction with { recebedor = recebedor };
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComValor(double valor)
    {
        _transaction = _transaction with { valor = valor };
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComTipoIniciacao(EnumTpIniciacao tipoIniciacao)
    {
        _transaction = _transaction with { tpIniciacao = tipoIniciacao };
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComChave(string chave)
    {
        _transaction = _transaction with { chave = chave };
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComPrioridade(EnumPrioridadePagamento? prioridade)
    {
        _transaction = _transaction with { prioridadePagamento = prioridade };
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComFinalidade(EnumTipoFinalidade? finalidade)
    {
        _transaction = _transaction with { finalidade = finalidade };
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComCpfPagadorInvalido()
    {
        if (_transaction.pagador != null)
        {
            _transaction.pagador.cpfCnpj = 123123123;
        }
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComValorDetalhe(List<JDPIValorDetalhe> valorDetalhe)
    {
        _transaction = _transaction with { vlrDetalhe = valorDetalhe };
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComCorrelationId(string correlationId)
    {
        _transaction.CorrelationId = correlationId;
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComCanal(int canal)
    {
        _transaction.canal = canal;
        return this;
    }

    public TransactionRegistrarOrdemPagamentoBuilder ComChaveIdempotencia(string chaveIdempotencia)
    {
        _transaction.chaveIdempotencia = chaveIdempotencia;
        return this;
    }

    public TransactionRegistrarOrdemPagamento Build()
    {
        return _transaction;
    }
}

public class TransactionEfetivarOrdemPagamentoBuilder
{
    private TransactionEfetivarOrdemPagamento _transaction;
    private static readonly Fixture _fixture = new();

    public TransactionEfetivarOrdemPagamentoBuilder()
    {
        _transaction = new TransactionEfetivarOrdemPagamento
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString(),
            Code = 2,
            canal = 26,
            chaveIdempotencia = "IDEM123",
            idReqJdPi = "JDPI123456789",
            endToEndId = "E12345678901234567890123456789012",
            dtHrReqJdPi = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            agendamentoID = "AGENDA123"
        };
    }

    public TransactionEfetivarOrdemPagamentoBuilder ComDadosValidos()
    {
        return this;
    }

    public TransactionEfetivarOrdemPagamentoBuilder ComIdReqSistemaCliente(string idReqSistemaCliente)
    {
        _transaction = _transaction with { idReqSistemaCliente = idReqSistemaCliente };
        return this;
    }

    public TransactionEfetivarOrdemPagamentoBuilder ComIdReqJdPi(string idReqJdPi)
    {
        _transaction = _transaction with { idReqJdPi = idReqJdPi };
        return this;
    }

    public TransactionEfetivarOrdemPagamentoBuilder ComEndToEndId(string endToEndId)
    {
        _transaction = _transaction with { endToEndId = endToEndId };
        return this;
    }

    public TransactionEfetivarOrdemPagamentoBuilder ComDtHrReqJdPi(string dtHrReqJdPi)
    {
        _transaction = _transaction with { dtHrReqJdPi = dtHrReqJdPi };
        return this;
    }

    public TransactionEfetivarOrdemPagamentoBuilder ComAgendamentoID(string agendamentoID)
    {
        _transaction = _transaction with { agendamentoID = agendamentoID };
        return this;
    }

    public TransactionEfetivarOrdemPagamentoBuilder ComCorrelationId(string correlationId)
    {
        _transaction.CorrelationId = correlationId;
        return this;
    }

    public TransactionEfetivarOrdemPagamento Build()
    {
        return _transaction;
    }
}

public class TransactionCancelarOrdemPagamentoBuilder
{
    private TransactionCancelarOrdemPagamento _transaction;
    private static readonly Fixture _fixture = new();

    public TransactionCancelarOrdemPagamentoBuilder()
    {
        _transaction = new TransactionCancelarOrdemPagamento
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString(),
            Code = 3,
            canal = 26,
            chaveIdempotencia = "IDEM123",
            agendamentoID = "AGENDA123",
            motivo = "Cancelamento solicitado pelo cliente",
            tipoErro = EnumTipoErro.NEGOCIO
        };
    }

    public TransactionCancelarOrdemPagamentoBuilder ComDadosValidos()
    {
        return this;
    }

    public TransactionCancelarOrdemPagamentoBuilder ComIdReqSistemaCliente(string idReqSistemaCliente)
    {
        _transaction = _transaction with { idReqSistemaCliente = idReqSistemaCliente };
        return this;
    }

    public TransactionCancelarOrdemPagamentoBuilder ComAgendamentoID(string agendamentoID)
    {
        _transaction = _transaction with { agendamentoID = agendamentoID };
        return this;
    }

    public TransactionCancelarOrdemPagamentoBuilder ComMotivo(string motivo)
    {
        _transaction = _transaction with { motivo = motivo };
        return this;
    }

    public TransactionCancelarOrdemPagamentoBuilder ComTipoErro(EnumTipoErro tipoErro)
    {
        _transaction = _transaction with { tipoErro = tipoErro };
        return this;
    }

    public TransactionCancelarOrdemPagamentoBuilder ComCorrelationId(string correlationId)
    {
        _transaction.CorrelationId = correlationId;
        return this;
    }

    public TransactionCancelarOrdemPagamento Build()
    {
        return _transaction;
    }
}

#endregion

#region Devolução Builders

public class TransactionRegistrarOrdemDevolucaoBuilder
{
    private TransactionRegistrarOrdemDevolucao _transaction;
    private static readonly Fixture _fixture = new();

    public TransactionRegistrarOrdemDevolucaoBuilder()
    {
        _transaction = new TransactionRegistrarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString(),
            Code = 4,
            canal = 26,
            chaveIdempotencia = "IDEM123",
            endToEndIdOriginal = "E12345678901234567890123456789012",
            endToEndIdDevolucao = "D12345678901234567890123456789012",
            codigoDevolucao = "MD06",
            motivoDevolucao = "Devolução solicitada pelo cliente",
            valorDevolucao = 50.25
        };
    }

    public TransactionRegistrarOrdemDevolucaoBuilder ComDadosValidos()
    {
        return this;
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

    public TransactionRegistrarOrdemDevolucaoBuilder ComEndToEndIdDevolucao(string endToEndIdDevolucao)
    {
        _transaction = _transaction with { endToEndIdDevolucao = endToEndIdDevolucao };
        return this;
    }

    public TransactionRegistrarOrdemDevolucaoBuilder ComCodigoDevolucao(string codigoDevolucao)
    {
        _transaction = _transaction with { codigoDevolucao = codigoDevolucao };
        return this;
    }

    public TransactionRegistrarOrdemDevolucaoBuilder ComMotivoDevolucao(string motivoDevolucao)
    {
        _transaction = _transaction with { motivoDevolucao = motivoDevolucao };
        return this;
    }

    public TransactionRegistrarOrdemDevolucaoBuilder ComValorDevolucao(double valorDevolucao)
    {
        _transaction = _transaction with { valorDevolucao = valorDevolucao };
        return this;
    }

    public TransactionRegistrarOrdemDevolucaoBuilder ComCorrelationId(string correlationId)
    {
        _transaction.CorrelationId = correlationId;
        return this;
    }

    public TransactionRegistrarOrdemDevolucao Build()
    {
        return _transaction;
    }
}

public class TransactionEfetivarOrdemDevolucaoBuilder
{
    private TransactionEfetivarOrdemDevolucao _transaction;
    private static readonly Fixture _fixture = new();

    public TransactionEfetivarOrdemDevolucaoBuilder()
    {
        _transaction = new TransactionEfetivarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString(),
            Code = 6,
            canal = 26,
            chaveIdempotencia = "IDEM123",
            idReqJdPi = "JDPI123456789",
            endToEndIdOriginal = "E12345678901234567890123456789012",
            endToEndIdDevolucao = "D12345678901234567890123456789012",
            dtHrReqJdPi = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
        };
    }

    public TransactionEfetivarOrdemDevolucaoBuilder ComDadosValidos()
    {
        return this;
    }

    public TransactionEfetivarOrdemDevolucaoBuilder ComIdReqSistemaCliente(string idReqSistemaCliente)
    {
        _transaction = _transaction with { idReqSistemaCliente = idReqSistemaCliente };
        return this;
    }

    public TransactionEfetivarOrdemDevolucaoBuilder ComIdReqJdPi(string idReqJdPi)
    {
        _transaction = _transaction with { idReqJdPi = idReqJdPi };
        return this;
    }

    public TransactionEfetivarOrdemDevolucaoBuilder ComEndToEndIdOriginal(string endToEndIdOriginal)
    {
        _transaction = _transaction with { endToEndIdOriginal = endToEndIdOriginal };
        return this;
    }

    public TransactionEfetivarOrdemDevolucaoBuilder ComEndToEndIdDevolucao(string endToEndIdDevolucao)
    {
        _transaction = _transaction with { endToEndIdDevolucao = endToEndIdDevolucao };
        return this;
    }

    public TransactionEfetivarOrdemDevolucaoBuilder ComDtHrReqJdPi(string dtHrReqJdPi)
    {
        _transaction = _transaction with { dtHrReqJdPi = dtHrReqJdPi };
        return this;
    }

    public TransactionEfetivarOrdemDevolucaoBuilder ComCorrelationId(string correlationId)
    {
        _transaction.CorrelationId = correlationId;
        return this;
    }

    public TransactionEfetivarOrdemDevolucao Build()
    {
        return _transaction;
    }
}

public class TransactionCancelarOrdemDevolucaoBuilder
{
    private TransactionCancelarOrdemDevolucao _transaction;
    private static readonly Fixture _fixture = new();

    public TransactionCancelarOrdemDevolucaoBuilder()
    {
        _transaction = new TransactionCancelarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString(),
            Code = 5,
            canal = 26,
            chaveIdempotencia = "IDEM123"
        };
    }

    public TransactionCancelarOrdemDevolucaoBuilder ComDadosValidos()
    {
        return this;
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

    public TransactionCancelarOrdemDevolucaoBuilder ComCanal(int canal)
    {
        _transaction.canal = canal;
        return this;
    }

    public TransactionCancelarOrdemDevolucaoBuilder ComChaveIdempotencia(string chaveIdempotencia)
    {
        _transaction.chaveIdempotencia = chaveIdempotencia;
        return this;
    }

    public TransactionCancelarOrdemDevolucao Build()
    {
        return _transaction;
    }
}

#endregion