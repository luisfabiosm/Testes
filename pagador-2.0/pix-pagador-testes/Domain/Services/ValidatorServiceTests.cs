using Domain.Core.Enum;
using Domain.Core.Exceptions;
using Domain.Core.Models.JDPI;
using Domain.Services;
using Xunit;

namespace pix_pagador_testes.Domain.Services
{
    public class ValidatorServiceTests
    {
        private readonly ValidatorService _validatorService;

        public ValidatorServiceTests()
        {
            _validatorService = new ValidatorService();
        }

        #region ValidarPagadorTests

        [Fact]
        public void ValidarPagador_ComPagadorValido_DeveRetornarSemErros()
        {
            // Arrange
            var pagador = new JDPIDadosConta
            {
                ispb = 12345678,
                cpfCnpj = 12345678901,
                nome = "João Silva",
                tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
                tpConta = EnumTipoConta.CORRENTE
            };

            // Act
            var (errors, isValid) = _validatorService.ValidarPagador(pagador);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidarPagador_ComPagadorNulo_DeveRetornarErro()
        {
            // Arrange
            JDPIDadosConta pagador = null;

            // Act
            var (errors, isValid) = _validatorService.ValidarPagador(pagador);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("pagador", errors[0].campo);
            Assert.Equal("pagador deve ser preenchido", errors[0].mensagens);
        }

        [Fact]
        public void ValidarPagador_ComIspbZero_DeveRetornarErro()
        {
            // Arrange
            var pagador = new JDPIDadosConta
            {
                ispb = 0,
                cpfCnpj = 12345678901,
                nome = "João Silva",
                tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
                tpConta = EnumTipoConta.CORRENTE
            };

            // Act
            var (errors, isValid) = _validatorService.ValidarPagador(pagador);

            // Assert
            Assert.False(isValid);
            Assert.Contains(errors, e => e.campo == "pagador.ispb");
        }

        [Fact]
        public void ValidarPagador_ComCpfCnpjZero_DeveRetornarErro()
        {
            // Arrange
            var pagador = new JDPIDadosConta
            {
                ispb = 12345678,
                cpfCnpj = 0,
                nome = "João Silva",
                tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
                tpConta = EnumTipoConta.CORRENTE
            };

            // Act
            var (errors, isValid) = _validatorService.ValidarPagador(pagador);

            // Assert
            Assert.False(isValid);
            Assert.Contains(errors, e => e.campo == "pagador.cpfCnpj");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void ValidarPagador_ComNomeInvalido_DeveRetornarErro(string nomeInvalido)
        {
            // Arrange
            var pagador = new JDPIDadosConta
            {
                ispb = 12345678,
                cpfCnpj = 12345678901,
                nome = nomeInvalido,
                tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
                tpConta = EnumTipoConta.CORRENTE
            };

            // Act
            var (errors, isValid) = _validatorService.ValidarPagador(pagador);

            // Assert
            Assert.False(isValid);
            Assert.Contains(errors, e => e.campo == "pagador.nome");
        }

        [Theory]
        [InlineData(EnumTipoPessoa.PESSOA_FISICA)]
        [InlineData(EnumTipoPessoa.PESSOA_JURIDICA)]
        public void ValidarPagador_ComTipoPessoaValido_DeveRetornarSemErros(EnumTipoPessoa tipoPessoa)
        {
            // Arrange
            var pagador = new JDPIDadosConta
            {
                ispb = 12345678,
                cpfCnpj = 12345678901,
                nome = "João Silva",
                tpPessoa = tipoPessoa,
                tpConta = EnumTipoConta.CORRENTE
            };

            // Act
            var (errors, isValid) = _validatorService.ValidarPagador(pagador);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(EnumTipoConta.CORRENTE)]
        [InlineData(EnumTipoConta.SALARIO)]
        [InlineData(EnumTipoConta.POUPANCA)]
        [InlineData(EnumTipoConta.PAGAMENTO)]
        public void ValidarPagador_ComTipoContaValido_DeveRetornarSemErros(EnumTipoConta tipoConta)
        {
            // Arrange
            var pagador = new JDPIDadosConta
            {
                ispb = 12345678,
                cpfCnpj = 12345678901,
                nome = "João Silva",
                tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
                tpConta = tipoConta
            };

            // Act
            var (errors, isValid) = _validatorService.ValidarPagador(pagador);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidarPagador_ComMultiplosErros_DeveRetornarTodosOsErros()
        {
            // Arrange
            var pagador = new JDPIDadosConta
            {
                ispb = 0,
                cpfCnpj = 0,
                nome = "",
                tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
                tpConta = EnumTipoConta.CORRENTE
            };

            // Act
            var (errors, isValid) = _validatorService.ValidarPagador(pagador);

            // Assert
            Assert.False(isValid);
            Assert.True(errors.Count >= 3); // ispb, cpfCnpj, nome
            Assert.Contains(errors, e => e.campo == "pagador.ispb");
            Assert.Contains(errors, e => e.campo == "pagador.cpfCnpj");
            Assert.Contains(errors, e => e.campo == "pagador.nome");
        }

        #endregion

        #region ValidarRecebedorTests

        [Fact]
        public void ValidarRecebedor_ComRecebedorValido_DeveRetornarSemErros()
        {
            // Arrange
            var recebedor = new JDPIDadosConta
            {
                ispb = 87654321,
                cpfCnpj = 98765432109,
                nome = "Maria Santos",
                tpPessoa = EnumTipoPessoa.PESSOA_JURIDICA,
                tpConta = EnumTipoConta.POUPANCA
            };

            // Act
            var (errors, isValid) = _validatorService.ValidarRecebedor(recebedor);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidarRecebedor_ComRecebedorNulo_DeveRetornarErro()
        {
            // Arrange
            JDPIDadosConta recebedor = null;

            // Act
            var (errors, isValid) = _validatorService.ValidarRecebedor(recebedor);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("recebedor", errors[0].campo);
            Assert.Equal("recebedor deve ser preenchido", errors[0].mensagens);
        }

        [Fact]
        public void ValidarRecebedor_ComIspbZero_DeveRetornarErro()
        {
            // Arrange
            var recebedor = new JDPIDadosConta
            {
                ispb = 0,
                cpfCnpj = 98765432109,
                tpPessoa = EnumTipoPessoa.PESSOA_JURIDICA,
                tpConta = EnumTipoConta.POUPANCA
            };

            // Act
            var (errors, isValid) = _validatorService.ValidarRecebedor(recebedor);

            // Assert
            Assert.False(isValid);
            Assert.Contains(errors, e => e.campo == "recebedor.ispb");
        }

        [Fact]
        public void ValidarRecebedor_ComCpfCnpjZero_DeveRetornarErro()
        {
            // Arrange
            var recebedor = new JDPIDadosConta
            {
                ispb = 87654321,
                cpfCnpj = 0,
                tpPessoa = EnumTipoPessoa.PESSOA_JURIDICA,
                tpConta = EnumTipoConta.POUPANCA
            };

            // Act
            var (errors, isValid) = _validatorService.ValidarRecebedor(recebedor);

            // Assert
            Assert.False(isValid);
            Assert.Contains(errors, e => e.campo == "recebedor.cpfCnpj");
        }

        #endregion

        #region ValidardtHrOpTests

        [Theory]
        [InlineData("2024-01-01T10:30:00")]
        [InlineData("25/12/2024 14:30:00")]
        [InlineData("01/01/2025")]
        [InlineData("teste")]
        public void ValidardtHrOp_ComValorValido_DeveRetornarSemErros(string dtHrOp)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidardtHrOp(dtHrOp);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void ValidardtHrOp_ComValorInvalido_DeveRetornarErro(string dtHrOpInvalido)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidardtHrOp(dtHrOpInvalido);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("dtHrOp", errors[0].campo);
            Assert.Contains("dtHrOp deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        #endregion

        #region ValidarValorTests

        [Theory]
        [InlineData(100.00)]
        [InlineData(0.01)]
        [InlineData(999999.99)]
        [InlineData(50.50)]
        public void ValidarValor_ComValorValido_DeveRetornarSemErros(double valor)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarValor(valor);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidarValor_ComValorZero_DeveRetornarErro()
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarValor(0.0);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("valor", errors[0].campo);
            Assert.Contains("valor deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        #endregion

        #region ValidarValorDevolucaoTests

        [Theory]
        [InlineData(100.00)]
        [InlineData(0.01)]
        [InlineData(999999.99)]
        [InlineData(50.50)]
        public void ValidarValorDevolucao_ComValorValido_DeveRetornarSemErros(double valor)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarValorDevolucao(valor);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidarValorDevolucao_ComValorZero_DeveRetornarErro()
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarValorDevolucao(0.0);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("valor", errors[0].campo);
            Assert.Contains("valor deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        #endregion

        #region ValidarChaveIdempotenciaTests

        [Theory]
        [InlineData("ABC123")]
        [InlineData("12345")]
        [InlineData("key-123-abc")]
        [InlineData("GUID-12345-67890")]
        public void ValidarChaveIdempotencia_ComChaveValida_DeveRetornarSemErros(string chave)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarChaveIdempotencia(chave);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void ValidarChaveIdempotencia_ComChaveInvalida_DeveRetornarErro(string chaveInvalida)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarChaveIdempotencia(chaveInvalida);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("chaveIdempotencia", errors[0].campo);
            Assert.Contains("chaveIdempotencia deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        #endregion

        #region ValidarEndToEndIdOriginalTests

        [Theory]
        [InlineData("E12345678901234567890123456789012")]
        [InlineData("E00038166202112261420u6db4t7t9j26")]
        [InlineData("E123456789012345")]
        public void ValidarEndToEndIdOriginal_ComIdValido_DeveRetornarSemErros(string endToEndId)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarEndToEndIdOriginal(endToEndId);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void ValidarEndToEndIdOriginal_ComIdInvalido_DeveRetornarErro(string endToEndIdInvalido)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarEndToEndIdOriginal(endToEndIdInvalido);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("endToEndIdOriginal", errors[0].campo);
            Assert.Contains("endToEndIdOriginal deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        #endregion

        #region ValidarEndToEndIdTests

        [Theory]
        [InlineData("E12345678901234567890123456789012")]
        [InlineData("E00038166202112261420u6db4t7t9j26")]
        [InlineData("E123456789012345")]
        public void ValidarEndToEndId_ComIdValido_DeveRetornarSemErros(string endToEndId)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarEndToEndId(endToEndId);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void ValidarEndToEndId_ComIdInvalido_DeveRetornarErro(string endToEndIdInvalido)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarEndToEndId(endToEndIdInvalido);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("endToEndId", errors[0].campo);
            Assert.Contains("endToEndId deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        #endregion

        #region ValidarCodigoDevolucaoTests

        [Theory]
        [InlineData("MD06")]
        [InlineData("AC03")]
        [InlineData("AG01")]
        [InlineData("TESTE")]
        public void ValidarCodigoDevolucao_ComCodigoValido_DeveRetornarSemErros(string codigo)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarCodigoDevolucao(codigo);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void ValidarCodigoDevolucao_ComCodigoInvalido_DeveRetornarErro(string codigoInvalido)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarCodigoDevolucao(codigoInvalido);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("codigoDevolucao", errors[0].campo);
            Assert.Contains("codigoDevolucao deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        #endregion

        #region ValidarIdReqSistemaClienteTests

        [Theory]
        [InlineData("REQ123")]
        [InlineData("12345")]
        [InlineData("SYS-CLIENT-001")]
        [InlineData("ABC-123-XYZ")]
        public void ValidarIdReqSistemaCliente_ComIdValido_DeveRetornarSemErros(string id)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarIdReqSistemaCliente(id);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void ValidarIdReqSistemaCliente_ComIdInvalido_DeveRetornarErro(string idInvalido)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarIdReqSistemaCliente(idInvalido);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("idReqSistemaCliente", errors[0].campo);
            Assert.Contains("idReqSistemaCliente deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        #endregion

        #region ValidarTpIniciacaoTests

        [Theory]
        
        [InlineData(EnumTpIniciacao.CHAVE)]
        [InlineData(EnumTpIniciacao.QR_CODE_ESTATICO)]
        [InlineData(EnumTpIniciacao.QR_CODE_DINAMICO)]
        [InlineData(EnumTpIniciacao.LINK_QR_ESTATICO)]
        [InlineData(EnumTpIniciacao.LINK_QR_DINAMICO)]
        [InlineData(EnumTpIniciacao.SERVICO_INI_TRANSACAO_PAG)]
        [InlineData(EnumTpIniciacao.QR_CODE_PAGADOR)]
        public void ValidarTpIniciacao_ComTipoValido_DeveRetornarSemErros(EnumTpIniciacao tpIniciacao)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarTpIniciacao(tpIniciacao);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidarTpIniciacao_ComTipoInvalido_DeveRetornarErro()
        {
            // Arrange
            var tpIniciacaoInvalido = (EnumTpIniciacao)999; // Valor inválido

            // Act
            var (errors, isValid) = _validatorService.ValidarTpIniciacao(tpIniciacaoInvalido);

            // Assert
            Assert.False(isValid);
            Assert.Contains(errors, e => e.campo == "tpIniciacao");
            Assert.Contains(errors, e => e.mensagens.Contains("domínio válido"));
        }

        #endregion

        #region ValidarMotivoTests

        [Theory]
        [InlineData("Pagamento de serviços")]
        [InlineData("Transferência entre contas")]
        [InlineData("Compra online")]
        [InlineData("X")]
        public void ValidarMotivo_ComMotivoValido_DeveRetornarSemErros(string motivo)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarMotivo(motivo);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void ValidarMotivo_ComMotivoInvalido_DeveRetornarErro(string motivoInvalido)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarMotivo(motivoInvalido);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("motivo", errors[0].campo);
            Assert.Contains("motivo deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        #endregion

        #region ValidarPrioridadePagamentoTests

        [Theory]
        [InlineData(EnumPrioridadePagamento.LIQUIDACAO_PRIORITARIA)]
        [InlineData(EnumPrioridadePagamento.LIQUIDACAO_NAO_PRIORITARIA)]
        public void ValidarPrioridadePagamento_ComPrioridadeValida_DeveRetornarSemErros(EnumPrioridadePagamento prioridade)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarPrioridadePagamento(prioridade);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidarPrioridadePagamento_ComPrioridadeNula_DeveRetornarErro()
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarPrioridadePagamento(null);

            // Assert
            Assert.False(isValid);
            Assert.Equal("prioridadePagamento", errors[0].campo);
            Assert.Contains("prioridadePagamento deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        [Fact]
        public void ValidarPrioridadePagamento_ComPrioridadeInvalida_DeveRetornarErro()
        {
            // Arrange
            var prioridadeInvalida = (EnumPrioridadePagamento?)999; // Valor inválido

            // Act
            var (errors, isValid) = _validatorService.ValidarPrioridadePagamento(prioridadeInvalida);

            // Assert
            Assert.False(isValid);
            Assert.Contains(errors, e => e.campo == "prioridadePagamento");
            Assert.Contains(errors, e => e.mensagens.Contains("domínio válido"));
        }

        #endregion

        #region ValidarTipoPrioridadePagamentoTests

        [Theory]
        [InlineData(EnumTpPrioridadePagamento.PAGAMENTO_PRIORITARIO)]
        [InlineData(EnumTpPrioridadePagamento.PAGAMENTO_ANALISE_ANTIFRAUDE)]
        [InlineData(EnumTpPrioridadePagamento.PAGAMENTO_AGENDADO)]
        public void ValidarTipoPrioridadePagamento_ComTipoValido_DeveRetornarSemErros(EnumTpPrioridadePagamento tipoPrioridade)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarTipoPrioridadePagamento(tipoPrioridade);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidarTipoPrioridadePagamento_ComTipoNulo_DeveRetornarErro()
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarTipoPrioridadePagamento(null);

            // Assert
            Assert.False(isValid);
            Assert.Equal("tpPrioridadePagamento", errors[0].campo);
            Assert.Contains("tpPrioridadePagamento deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        [Fact]
        public void ValidarTipoPrioridadePagamento_ComTipoInvalido_DeveRetornarErro()
        {
            // Arrange
            var tipoInvalido = (EnumTpPrioridadePagamento?)999; // Valor inválido

            // Act
            var (errors, isValid) = _validatorService.ValidarTipoPrioridadePagamento(tipoInvalido);

            // Assert
            Assert.False(isValid);
            Assert.Contains(errors, e => e.campo == "tpPrioridadePagamento");
            Assert.Contains(errors, e => e.mensagens.Contains("domínio válido"));
        }

        #endregion

        #region ValidarFinalidadeTests

        [Theory]
        [InlineData(EnumTipoFinalidade.COMPRA_OU_TRANSFERENCIA)]
        [InlineData(EnumTipoFinalidade.PIX_TROCO)]
        [InlineData(EnumTipoFinalidade.PIX_SAQUE)]
        public void ValidarFinalidade_ComFinalidadeValida_DeveRetornarSemErros(EnumTipoFinalidade finalidade)
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarFinalidade(finalidade);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidarFinalidade_ComFinalidadeNula_DeveRetornarErro()
        {
            // Act
            var (errors, isValid) = _validatorService.ValidarFinalidade(null);

            // Assert
            Assert.False(isValid);
          //  Assert.Single(errors);
            Assert.Equal("finalidade", errors[0].campo);
            Assert.Contains("finalidade deve ser informado e nao pode ser nulo", errors[0].mensagens);
        }

        [Fact]
        public void ValidarFinalidade_ComFinalidadeInvalida_DeveRetornarErro()
        {
            // Arrange
            var finalidadeInvalida = (EnumTipoFinalidade?)999; // Valor inválido

            // Act
            var (errors, isValid) = _validatorService.ValidarFinalidade(finalidadeInvalida);

            // Assert
            Assert.False(isValid);
            Assert.Contains(errors, e => e.campo == "finalidade");
            Assert.Contains(errors, e => e.mensagens.Contains("domínio válido"));
        }

        #endregion

        #region MetodosPrivados_Integration_Tests

        [Fact]
        public void ValidateRequired_ComStringVazia_DeveAdicionarErro()
        {
            // Arrange
            var errors = new List<ErrorDetails>();
            var fieldName = "testField";
            var emptyValue = "";

            // Usando reflexão para testar método privado através de um método público
            var pagador = new JDPIDadosConta
            {
                ispb = 12345678,
                cpfCnpj = 12345678901,
                nome = emptyValue, // String vazia para testar ValidateRequired
                tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
                tpConta = EnumTipoConta.CORRENTE
            };

            // Act
            var (result_errors, isValid) = _validatorService.ValidarPagador(pagador);

            // Assert
            Assert.False(isValid);
            Assert.Contains(result_errors, e => e.campo == "pagador.nome");
        }

        [Fact]
        public void ValidateEnum_ComEnumInvalido_DeveAdicionarErro()
        {
            // Arrange
            var pagador = new JDPIDadosConta
            {
                ispb = 12345678,
                cpfCnpj = 12345678901,
                nome = "João Silva",
                tpPessoa = (EnumTipoPessoa)999, // Enum inválido
                tpConta = EnumTipoConta.CORRENTE
            };

            // Act
            var (errors, isValid) = _validatorService.ValidarPagador(pagador);

            // Assert
            Assert.False(isValid);
            Assert.Contains(errors, e => e.campo == "pagador.tpPessoa" && e.mensagens.Contains("domínio válido"));
        }

        #endregion

        #region CenariosCombinados_Tests

        [Fact]
        public void CenarioCompleto_ValidacaoPagadorERecebedor_ComDadosValidos()
        {
            // Arrange
            var pagador = new JDPIDadosConta
            {
                ispb = 12345678,
                cpfCnpj = 12345678901,
                nome = "João Silva",
                tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
                tpConta = EnumTipoConta.CORRENTE
            };

            var recebedor = new JDPIDadosConta
            {
                ispb = 87654321,
                cpfCnpj = 98765432109,
                nome = "Maria Santos",
                tpPessoa = EnumTipoPessoa.PESSOA_JURIDICA,
                tpConta = EnumTipoConta.POUPANCA
            };

            // Act
            var (errorsPagador, isValidPagador) = _validatorService.ValidarPagador(pagador);
            var (errorsRecebedor, isValidRecebedor) = _validatorService.ValidarRecebedor(recebedor);
            var (errorsValor, isValidValor) = _validatorService.ValidarValor(150.50);
            var (errorsMotivo, isValidMotivo) = _validatorService.ValidarMotivo("Pagamento de serviços");

            // Assert
            Assert.True(isValidPagador);
            Assert.True(isValidRecebedor);
            Assert.True(isValidValor);
            Assert.True(isValidMotivo);
            Assert.Empty(errorsPagador);
            Assert.Empty(errorsRecebedor);
            Assert.Empty(errorsValor);
            Assert.Empty(errorsMotivo);
        }

        [Fact]
        public void CenarioCompleto_ValidacaoComTodosOsEnums_ComValoresValidos()
        {
            // Act
            var (errorsTpIniciacao, isValidTpIniciacao) = _validatorService.ValidarTpIniciacao(EnumTpIniciacao.QR_CODE_DINAMICO);
            var (errorsPrioridade, isValidPrioridade) = _validatorService.ValidarPrioridadePagamento(EnumPrioridadePagamento.LIQUIDACAO_PRIORITARIA);
            var (errorsTipoPrioridade, isValidTipoPrioridade) = _validatorService.ValidarTipoPrioridadePagamento(EnumTpPrioridadePagamento.PAGAMENTO_PRIORITARIO);
            var (errorsFinalidade, isValidFinalidade) = _validatorService.ValidarFinalidade(EnumTipoFinalidade.COMPRA_OU_TRANSFERENCIA);

            // Assert
            Assert.True(isValidTpIniciacao);
            Assert.True(isValidPrioridade);
            Assert.True(isValidTipoPrioridade);
            Assert.True(isValidFinalidade);
            Assert.Empty(errorsTpIniciacao);
            Assert.Empty(errorsPrioridade);
            Assert.Empty(errorsTipoPrioridade);
            Assert.Empty(errorsFinalidade);
        }

        #endregion
    }
}