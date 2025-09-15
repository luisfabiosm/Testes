using Domain.Core.Enum;
using Domain.Core.Models.JDPI;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;


namespace pix_pagador_testes.Domain.UseCases.Pagamento
{


    public class TransactionRegistrarOrdemPagamentoBuilder
    {
        private TransactionRegistrarOrdemPagamento _transaction;

        public TransactionRegistrarOrdemPagamentoBuilder()
        {
            _transaction = new TransactionRegistrarOrdemPagamento
            {
                idReqSistemaCliente = "REQ123456789",
                CorrelationId = Guid.NewGuid().ToString(),
                Code = 1,
                canal = 26,
                chaveIdempotencia = Guid.NewGuid().ToString(),
                pagador = CreateDefaultPagador(),
                recebedor = CreateDefaultRecebedor(),
                tpIniciacao = EnumTpIniciacao.CHAVE,
                valor = 100.50,
                infEntreClientes = "Pagamento de teste",
                prioridadePagamento = EnumPrioridadePagamento.LIQUIDACAO_NAO_PRIORITARIA,
                tpPrioridadePagamento = EnumTpPrioridadePagamento.PAGAMENTO_AGENDADO,
                finalidade = EnumTipoFinalidade.PIX_SAQUE,
                modalidadeAgente = EnumModalidadeAgente.AGENTE_PRESTADOR_SERVICO_SAQUE,
                agendamentoID = "AGENDA123",
                endToEndId = "E12345678901234567890123456789012",
                dtEnvioPag = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                chave = "user@example.com"
            };
        }

        public TransactionRegistrarOrdemPagamentoBuilder ComIdReqSistemaCliente(string idReqSistemaCliente)
        {
            _transaction = _transaction with { idReqSistemaCliente = idReqSistemaCliente };
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

        public TransactionRegistrarOrdemPagamentoBuilder ComTpIniciacao(EnumTpIniciacao tpIniciacao)
        {
            _transaction = _transaction with { tpIniciacao = tpIniciacao };
            return this;
        }

        public TransactionRegistrarOrdemPagamentoBuilder ComInfEntreClientes(string infEntreClientes)
        {
            _transaction = _transaction with { infEntreClientes = infEntreClientes };
            return this;
        }

        public TransactionRegistrarOrdemPagamentoBuilder ComPrioridadePagamento(EnumPrioridadePagamento? prioridadePagamento)
        {
            _transaction = _transaction with { prioridadePagamento = prioridadePagamento };
            return this;
        }

        public TransactionRegistrarOrdemPagamentoBuilder ComFinalidade(EnumTipoFinalidade? finalidade)
        {
            _transaction = _transaction with { finalidade = finalidade };
            return this;
        }

        public TransactionRegistrarOrdemPagamentoBuilder ComEndToEndId(string endToEndId)
        {
            _transaction = _transaction with { endToEndId = endToEndId };
            return this;
        }

        public TransactionRegistrarOrdemPagamentoBuilder ComChave(string chave)
        {
            _transaction = _transaction with { chave = chave };
            return this;
        }

        public TransactionRegistrarOrdemPagamentoBuilder ComAgendamentoID(string agendamentoID)
        {
            _transaction = _transaction with { agendamentoID = agendamentoID };
            return this;
        }

        public TransactionRegistrarOrdemPagamentoBuilder ComCorrelationId(string correlationId)
        {
            _transaction.CorrelationId = correlationId;
            return this;
        }

        public TransactionRegistrarOrdemPagamento Build() => _transaction;

        private static JDPIDadosConta CreateDefaultPagador()
        {
            return new JDPIDadosConta
            {
                ispb = 12345678,
                cpfCnpj = 12345678901,
                nome = "João Silva",
                tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
                tpConta = EnumTipoConta.CORRENTE,
                nrAgencia = "1234",
                nrConta = "567890"
            };
        }

        private static JDPIDadosConta CreateDefaultRecebedor()
        {
            return new JDPIDadosConta
            {
                ispb = 87654321,
                cpfCnpj = 98765432100,
                nome = "Maria Santos",
                tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
                tpConta = EnumTipoConta.CORRENTE,
                nrAgencia = "321",
                nrConta = "098765"
            };
        }
    }
}
