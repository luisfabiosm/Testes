using Domain.Services;
using Xunit;

namespace pix_pagador_testes.Domain.Services
{
    public class CorrelationIdGeneratorTests
    {
        private readonly CorrelationIdGenerator _generator;

        public CorrelationIdGeneratorTests()
        {
            _generator = new CorrelationIdGenerator();
        }

        #region GenerateTests

        [Fact]
        public void Generate_DeveRetornarStringComTamanhoCorreto()
        {
            // Arrange
            const int tamanhoEsperado = 16;

            // Act
            var resultado = _generator.Generate();

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(tamanhoEsperado, resultado.Length);
        }

        [Fact]
        public void Generate_ComTamanhoPersonalizado_DeveRetornarStringComTamanhoCorreto()
        {
            // Arrange
            const int tamanhoPersonalizado = 24;

            // Act
            var resultado = _generator.Generate(tamanhoPersonalizado);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(tamanhoPersonalizado, resultado.Length);
        }

        [Fact]
        public void Generate_DeveRetornarValoresDiferentes_EmChamadasSequenciais()
        {
            // Arrange & Act
            var primeiro = _generator.Generate();
            var segundo = _generator.Generate();
            var terceiro = _generator.Generate();

            // Assert
            Assert.NotEqual(primeiro, segundo);
            Assert.NotEqual(segundo, terceiro);
            Assert.NotEqual(primeiro, terceiro);
        }

        [Fact]
        public void Generate_DeveConterApenasCaracteresValidos()
        {
            // Arrange
            const string caracteresValidos = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";

            // Act
            var resultado = _generator.Generate(32);

            // Assert
            Assert.NotNull(resultado);
            Assert.All(resultado, c => Assert.Contains(c, caracteresValidos));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(8)]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(64)]
        public void Generate_ComTamanhosValidos_DeveExecutarComSucesso(int tamanho)
        {
            // Act
            var resultado = _generator.Generate(tamanho);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(tamanho, resultado.Length);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(65)]
        [InlineData(100)]
        public void Generate_ComTamanhoInvalido_DeveLancarArgumentOutOfRangeException(int tamanhoInvalido)
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Generate(tamanhoInvalido));
            Assert.Equal("length", ex.ParamName);
            Assert.Contains("Tamanho precisa ser entre 1 e 64", ex.Message);
        }

        [Fact]
        public void Generate_ThreadSafety_DeveGerarIdsUnicos()
        {
            // Arrange
            const int numeroThreads = 10;
            const int idsPerThread = 100;
            var resultados = new List<string>();
            var tasks = new List<Task>();
            var lockObj = new object();

            // Act
            for (int i = 0; i < numeroThreads; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var idsLocais = new List<string>();
                    for (int j = 0; j < idsPerThread; j++)
                    {
                        idsLocais.Add(_generator.Generate());
                    }

                    lock (lockObj)
                    {
                        resultados.AddRange(idsLocais);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.Equal(numeroThreads * idsPerThread, resultados.Count);
            Assert.Equal(resultados.Count, resultados.Distinct().Count()); // Todos únicos
        }

        #endregion

        #region GenerateWithPrefixTests

        [Fact]
        public void GenerateWithPrefix_ComPrefixoValido_DeveRetornarStringComFormato()
        {
            // Arrange
            const string prefixo = "REQ";
            const int tamanhoId = 12;

            // Act
            var resultado = _generator.GenerateWithPrefix(prefixo, tamanhoId);

            // Assert
            Assert.NotNull(resultado);
            Assert.StartsWith($"{prefixo}-", resultado);
            Assert.Equal(prefixo.Length + 1 + tamanhoId, resultado.Length); // prefixo + "-" + id
        }

        [Fact]
        public void GenerateWithPrefix_ComPrefixoNulo_DeveRetornarApenasSemPrefixo()
        {
            // Arrange
            const int tamanhoId = 16;

            // Act
            var resultado = _generator.GenerateWithPrefix(null, tamanhoId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(tamanhoId, resultado.Length);
            Assert.DoesNotContain("-", resultado);
        }

        [Fact]
        public void GenerateWithPrefix_ComPrefixoVazio_DeveRetornarApenasSemPrefixo()
        {
            // Arrange
            const int tamanhoId = 16;

            // Act
            var resultado = _generator.GenerateWithPrefix(string.Empty, tamanhoId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(tamanhoId, resultado.Length);
            Assert.DoesNotContain("-", resultado);
        }

        [Theory]
        [InlineData("API")]
        [InlineData("SVC")]
        [InlineData("TXN")]
        [InlineData("LOG")]
        public void GenerateWithPrefix_ComDiferentesPrefixos_DeveRetornarFormatoCorreto(string prefixo)
        {
            // Act
            var resultado = _generator.GenerateWithPrefix(prefixo);

            // Assert
            Assert.NotNull(resultado);
            Assert.StartsWith($"{prefixo}-", resultado);
        }

        #endregion

        #region IsValidTests

        [Fact]
        public void IsValid_ComIdValido_DeveRetornarTrue()
        {
            // Arrange
            var idValido = _generator.Generate();

            // Act
            var resultado = _generator.IsValid(idValido);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public void IsValid_ComIdComPrefixo_DeveRetornarTrue()
        {
            // Arrange
            var idComPrefixo = _generator.GenerateWithPrefix("AP2");

            // Act
            var resultado = _generator.IsValid(idComPrefixo);

            // Assert
            Assert.True(resultado);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void IsValid_ComStringInvalida_DeveRetornarFalse(string idInvalido)
        {
            // Act
            var resultado = _generator.IsValid(idInvalido);

            // Assert
            Assert.False(resultado);
        }

        [Theory]
        [InlineData("ABC123@#$")]
        [InlineData("ABC123!")]
        [InlineData("ABC123*")]
        [InlineData("ABC123&")]
        [InlineData("ABC123%")]
        public void IsValid_ComCaracteresInvalidos_DeveRetornarFalse(string idComCaracteresInvalidos)
        {
            // Act
            var resultado = _generator.IsValid(idComCaracteresInvalidos);

            // Assert
            Assert.False(resultado);
        }

        [Theory]
        [InlineData("ABC234")]
        [InlineData("abc456")]
        [InlineData("ABCabc234")]
        [InlineData("AP2-ABC234")]
        [InlineData("SVC-xyz789")]
        [InlineData("TXN-ABC-DEF-234")]

        public void IsValid_ComCaracteresValidos_DeveRetornarTrue(string idValido)
        {
            // Act
            var resultado = _generator.IsValid(idValido);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public void IsValid_ComCaracteresExcluidosIlO_DeveRetornarFalse()
        {
            // Arrange - Testando caracteres excluídos para evitar confusão
            var idsComCaracteresExcluidos = new[] { "ABC1I3", "ABCLO3", "ABC0O3" };

            // Act & Assert
            foreach (var id in idsComCaracteresExcluidos)
            {
                var resultado = _generator.IsValid(id);
                Assert.False(resultado, $"ID '{id}' deveria ser inválido");
            }
        }

        #endregion

        #region PerformanceTests

        [Fact]
        public void Generate_Performance_DeveSerRapidoParaMultiplasGeracoes()
        {
            // Arrange
            const int numeroGeracoes = 10000;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < numeroGeracoes; i++)
            {
                _generator.Generate();
            }

            stopwatch.Stop();

            // Assert
            // Deve ser muito rápido - menos de 1 segundo para 10k gerações
            Assert.True(stopwatch.ElapsedMilliseconds < 1000,
                $"Performance inadequada: {stopwatch.ElapsedMilliseconds}ms para {numeroGeracoes} gerações");
        }

        #endregion
    }
}