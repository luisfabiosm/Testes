using Domain.Core.Models.SPA;
using System.Data.SqlClient;
using System.Data;

namespace Processador.Domain.Core.Models.SPA
{
    public class SPAParametroTests
    {
        [Fact]
        public void Construtor_ComSqlParameter_DeveInicializarCorretamente()
        {
            // Arrange
            var sqlParam = new SqlParameter("@param", SqlDbType.Int)
            {
                Direction = ParameterDirection.Input,
                Value = 42
            };

            // Act
            var spaParam = new SPAParametro(sqlParam, 1, true);

            // Assert
            Assert.Equal(1, spaParam.Indice);
            Assert.True(spaParam.Reservado);
            Assert.Equal("@param", spaParam.Nome);
            Assert.Equal(SqlDbType.Int, spaParam.Tipo);
            Assert.Equal(ParameterDirection.Input, spaParam.Direcao);
            Assert.Equal(42, spaParam.Valor);
        }

        [Fact]
        public void Construtor_ComValor_DeveSetarCorretamente()
        {
            // Act
            var spaParam = new SPAParametro(10, "testValue", false);

            // Assert
            Assert.Equal(10, spaParam.Indice);
            Assert.False(spaParam.Reservado);
        }

        #region RecuperarSqlParameterTests

        [Fact]
        public void RecuperarSqlParameterInput_DeveRetornarCopiaCorreta()
        {
            // Arrange
            var sqlParam = new SqlParameter("@x", SqlDbType.VarChar)
            {
                Size = 50,
                Direction = ParameterDirection.Input,
                IsNullable = true,
                Precision = 5,
                Scale = 2,
                SourceColumn = "col",
                SourceVersion = DataRowVersion.Default,
                Value = "teste"
            };
            var spaParam = new SPAParametro(sqlParam, 1);

            // Act
            var result = spaParam.RecuperarSqlParameterInput();

            // Assert
            Assert.Equal("@x", result.ParameterName);
            Assert.Equal("teste", result.Value);
            Assert.Equal(SqlDbType.VarChar, result.SqlDbType);
            Assert.Equal(50, result.Size);
        }

        [Fact]
        public void RecuperarSqlParameterOutput_DeveRetornarCorretamente()
        {
            // Arrange
            var sqlParam = new SqlParameter("@x", SqlDbType.Int)
            {
                Size = 0,
                Direction = ParameterDirection.Output
            };
            var spaParam = new SPAParametro(sqlParam, 1);

            // Act
            var result = spaParam.RecuperarSqlParameterOutput();

            // Assert
            Assert.Equal("@x", result.ParameterName);
            Assert.Equal(ParameterDirection.Output, result.Direction);
        }

        #endregion

        [Fact]
        public void Valor_DeveRetornarDataPadraoQuandoVazio()
        {
            var param = new SqlParameter("@data", SqlDbType.DateTime)
            {
                Direction = ParameterDirection.Input,
                Value = ""
            };
            var spaParam = new SPAParametro(param, 1);

            // Act
            var valor = spaParam.Valor;

            // Assert
            Assert.Equal("1/1/1900 12:00:00 AM", valor.ToString());
        }

        [Theory]
        [InlineData("123.00", 123)]
        [InlineData("123.0", 123)]
        [InlineData("123", 123)]
        [InlineData("-12345", 12345)]
        [InlineData("xyz", 0)]
        public void ParseDecimal_DeveConverterCorretamente(string input, decimal expected)
        {
            var spaParam = new SPAParametro(1, 0);
            var result = spaParam.ParseDecimal(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(SqlDbType.Bit, "1", 1)]
        [InlineData(SqlDbType.Bit, "0", 0)]
        [InlineData(SqlDbType.Int, "123", "123")]
        [InlineData(SqlDbType.TinyInt, "2", 2)]
        [InlineData(SqlDbType.Decimal, "12345", "12345")]
        [InlineData(SqlDbType.Float, "67890000.00", "67890000.00")]
        [InlineData(SqlDbType.VarChar, "Texto", "Texto")]
        public void Valor_SetGet_DeveTratarTiposSuportados(SqlDbType tipo, string input, object expectedOutput)
        {
            // Arrange
            var sqlParameter = new SqlParameter
            {
                ParameterName = "@p1",
                SqlDbType = tipo,
                Direction = ParameterDirection.Input,
                Size = 100
            };

            var parametro = new SPAParametro(sqlParameter, indice: 1);

            // Act
            parametro.Valor = input;
            var resultado = parametro.Valor;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(expectedOutput, resultado);
        }

        [Fact]
        public void Valor_NuloOuVazio_DeveRetornarDefault()
        {
            // Arrange
            var sqlParameter = new SqlParameter
            {
                ParameterName = "@p1",
                SqlDbType = SqlDbType.VarChar,
                Direction = ParameterDirection.Input,
                Value = "",
                Size = 100
            };

            var parametro = new SPAParametro(sqlParameter, indice: 25);

            // Act
            var valor = parametro.Valor;

            // Assert
            Assert.Equal("", valor);
        }

        [Fact]
        public void RecuperarSqlParameterInput_DeveRetornarNovoObjeto()
        {
            var sqlParameter = new SqlParameter("@p1", SqlDbType.Int)
            {
                Direction = ParameterDirection.Input,
                Value = 42
            };

            var parametro = new SPAParametro(sqlParameter, 1);
            var novoParametro = parametro.RecuperarSqlParameterInput();

            Assert.NotSame(sqlParameter, novoParametro);
            Assert.Equal("@p1", novoParametro.ParameterName);
            Assert.Equal(SqlDbType.Int, novoParametro.SqlDbType);
            Assert.Equal(ParameterDirection.Input, novoParametro.Direction);
        }

        [Fact]
        public void RecuperarSqlParameterOutput_DeveRetornarNovoOutput()
        {
            var sqlParameter = new SqlParameter("@p2", SqlDbType.VarChar)
            {
                Direction = ParameterDirection.Output,
                Size = 50
            };

            var parametro = new SPAParametro(sqlParameter, 2);
            var outputParam = parametro.RecuperarSqlParameterOutput();

            Assert.Equal("@p2", outputParam.ParameterName);
            Assert.Equal(SqlDbType.VarChar, outputParam.SqlDbType);
            Assert.Equal(ParameterDirection.Output, outputParam.Direction);
        }
    }
}
