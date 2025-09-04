using W3Socket.Lib;

namespace Componente.Lib
{
    public class StatePropertiesTests
    {
        [Fact]
        public void Indexador_DeveArmazenarERecuperarObjeto()
        {
            // Arrange
            var state = new StateProperties();
            var key = "prop1";
            var value = "teste";

            // Act
            state[key] = value;
            var resultado = state[key];

            // Assert
            Assert.Equal(value, resultado);
        }

        [Fact]
        public void Indexador_DeveSobrescreverValorExistente()
        {
            // Arrange
            var state = new StateProperties();
            var key = "prop1";

            state[key] = "valor1";
            state[key] = "valor2";

            // Act
            var resultado = state[key];

            // Assert
            Assert.Equal("valor2", resultado);
        }

        [Fact]
        public void Indexador_DeveRemoverValorQuandoAtribuidoNull()
        {
            // Arrange
            var state = new StateProperties();
            var key = "prop1";
            state[key] = "valor1";

            // Act
            state[key] = null;
            var resultado = state[key];

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public void GetRpi_DeveRetornarObjetoDoTipoRemoteProcInfo()
        {
            // Arrange
            var state = new StateProperties();
            var key = "procX";
            var rpi = new RemoteProcInfo(); // supondo que exista uma classe com esse nome

            state[key] = rpi;

            // Act
            var resultado = state.GetRpi(key);

            // Assert
            Assert.NotNull(resultado);
            Assert.IsType<RemoteProcInfo>(resultado);
            Assert.Equal(rpi, resultado);
        }

        [Fact]
        public void GetRpi_DeveRetornarNull_SeChaveNaoExistir()
        {
            // Arrange
            var state = new StateProperties();

            // Act
            var resultado = state.GetRpi("naoExiste");

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public void Indexador_NaoDeveLancarErroParaChaveInexistente()
        {
            // Arrange
            var state = new StateProperties();

            // Act
            var resultado = state["naoExiste"];

            // Assert
            Assert.Null(resultado);
        }
    }
}
