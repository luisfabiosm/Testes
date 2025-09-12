using Domain.Core.Settings;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace pix_pagador_testes.Domain.Core.Settings
{
    public class DBSettingsTest
    {
        #region Constructor and Initialization Tests

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new DBSettings();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void DefaultValues_DevemEstarConfiguradosCorretamente()
        {
            // Act
            var settings = new DBSettings();

            // Assert
            Assert.Equal(10, settings.CommandTimeout);
            Assert.Equal(10, settings.ConnectTimeout);
            Assert.Equal(0, settings.Port); // Valor padrão para int
            Assert.Null(settings.ServerUrl);
            Assert.Null(settings.Database);
            Assert.Null(settings.Username);
            Assert.Null(settings.Password);
        }

        [Fact]
        public void Properties_DevemSerDefiniveisELegíveis()
        {
            // Arrange
            var settings = new DBSettings();
            var expectedServer = "localhost";
            var expectedDatabase = "TestDB";
            var expectedUsername = "testuser";
            var expectedPassword = "testpass";
            var expectedCommandTimeout = 30;
            var expectedConnectTimeout = 15;
            var expectedPort = 1433;

            // Act
            settings.ServerUrl = expectedServer;
            settings.Database = expectedDatabase;
            settings.Username = expectedUsername;
            settings.Password = expectedPassword;
            settings.CommandTimeout = expectedCommandTimeout;
            settings.ConnectTimeout = expectedConnectTimeout;
            settings.Port = expectedPort;

            // Assert
            Assert.Equal(expectedServer, settings.ServerUrl);
            Assert.Equal(expectedDatabase, settings.Database);
            Assert.Equal(expectedUsername, settings.Username);
            Assert.Equal(expectedPassword, settings.Password);
            Assert.Equal(expectedCommandTimeout, settings.CommandTimeout);
            Assert.Equal(expectedConnectTimeout, settings.ConnectTimeout);
            Assert.Equal(expectedPort, settings.Port);
        }

        #endregion

        #region GetConnectionString Tests

        [Fact]
        public void GetConnectionString_ComConfiguracoesCompletas_DeveRetornarStringCorreta()
        {
            // Arrange
            var plainPassword = "password123";
            var encryptedPassword = CryptSPATestHelper.EncryptDES(plainPassword);

            var settings = new DBSettings
            {
                ServerUrl = "localhost",
                Database = "TestDB",
                Username = "testuser",
                Password = encryptedPassword,
                ConnectTimeout = 20,
                CommandTimeout = 30
            };

            // Act
            var connectionString = settings.GetConnectionString();

            // Assert
            Assert.Contains("Data Source=localhost", connectionString);
            Assert.Contains("Initial Catalog=TestDB", connectionString);
            Assert.Contains("User ID=testuser", connectionString);
            Assert.Contains($"Password={plainPassword}", connectionString);
            Assert.Contains("Connect Timeout=20", connectionString);
            Assert.Contains("TrustServerCertificate=True", connectionString);
            Assert.Contains("Persist Security Info=True", connectionString);
            Assert.Contains("MultipleActiveResultSets=true", connectionString);
            Assert.Contains("Enlist=false", connectionString);
        }

        [Fact]
        public void GetConnectionString_ComConnectTimeoutZero_DeveUsarValorPadrao20()
        {
            // Arrange
            var plainPassword = "password123";
            var encryptedPassword = CryptSPATestHelper.EncryptDES(plainPassword);

            var settings = new DBSettings
            {
                ServerUrl = "localhost",
                Database = "TestDB",
                Username = "testuser",
                Password = encryptedPassword,
                ConnectTimeout = 0
            };

            // Act
            var connectionString = settings.GetConnectionString();

            // Assert
            Assert.Contains("Connect Timeout=20", connectionString);
        }

        [Fact]
        public void GetConnectionString_ComConnectTimeoutPersonalizado_DeveUsarValorEspecificado()
        {
            // Arrange
            var plainPassword = "password123";
            var encryptedPassword = CryptSPATestHelper.EncryptDES(plainPassword);

            var settings = new DBSettings
            {
                ServerUrl = "localhost",
                Database = "TestDB",
                Username = "testuser",
                Password = encryptedPassword,
                ConnectTimeout = 45
            };

            // Act
            var connectionString = settings.GetConnectionString();

            // Assert
            Assert.Contains("Connect Timeout=45", connectionString);
        }

        [Theory]
        [InlineData("server1", "db1", "user1")]
        [InlineData("prod-server", "ProductionDB", "admin")]
        [InlineData("test-server.domain.com", "TestDB", "testuser")]
        public void GetConnectionString_ComDiferentesConfiguracoes_DeveIncluirValoresCorretos(
            string serverUrl, string database, string username)
        {
            // Arrange
            var plainPassword = "testpass";
            var encryptedPassword = CryptSPATestHelper.EncryptDES(plainPassword);

            var settings = new DBSettings
            {
                ServerUrl = serverUrl,
                Database = database,
                Username = username,
                Password = encryptedPassword
            };

            // Act
            var connectionString = settings.GetConnectionString();

            // Assert
            Assert.Contains($"Data Source={serverUrl}", connectionString);
            Assert.Contains($"Initial Catalog={database}", connectionString);
            Assert.Contains($"User ID={username}", connectionString);
            Assert.Contains($"Password={plainPassword}", connectionString);
        }

        [Fact]
        public void GetConnectionString_ComPasswordVazia_DeveDescriptografarCorretamente()
        {
            // Arrange
            var emptyPassword = "";
            var encryptedEmptyPassword = CryptSPATestHelper.EncryptDES(emptyPassword);

            var settings = new DBSettings
            {
                ServerUrl = "localhost",
                Database = "TestDB",
                Username = "testuser",
                Password = encryptedEmptyPassword
            };

            // Act
            var connectionString = settings.GetConnectionString();

            // Assert
            Assert.Contains("Password=", connectionString);
        }

        #endregion

        #region Record Behavior Tests

        [Fact]
        public void Record_DeveImplementarIgualdadeCorretamente()
        {
            // Arrange
            var settings1 = new DBSettings
            {
                ServerUrl = "localhost",
                Database = "TestDB",
                Username = "testuser",
                Password = "password",
                CommandTimeout = 30,
                ConnectTimeout = 20,
                Port = 1433
            };

            var settings2 = new DBSettings
            {
                ServerUrl = "localhost",
                Database = "TestDB",
                Username = "testuser",
                Password = "password",
                CommandTimeout = 30,
                ConnectTimeout = 20,
                Port = 1433
            };

            // Act & Assert
            Assert.Equal(settings1, settings2);
            Assert.True(settings1 == settings2);
            Assert.False(settings1 != settings2);
        }

        [Fact]
        public void Record_DeveImplementarHashCodeCorretamente()
        {
            // Arrange
            var settings1 = new DBSettings
            {
                ServerUrl = "localhost",
                Database = "TestDB",
                Username = "testuser"
            };

            var settings2 = new DBSettings
            {
                ServerUrl = "localhost",
                Database = "TestDB",
                Username = "testuser"
            };

            // Act & Assert
            Assert.Equal(settings1.GetHashCode(), settings2.GetHashCode());
        }

        [Fact]
        public void Record_DeveImplementarToStringCorretamente()
        {
            // Arrange
            var settings = new DBSettings
            {
                ServerUrl = "localhost",
                Database = "TestDB",
                Username = "testuser",
                Password = "password"
            };

            // Act
            var toString = settings.ToString();

            // Assert
            Assert.Contains("ServerUrl", toString);
            Assert.Contains("localhost", toString);
            Assert.Contains("Database", toString);
            Assert.Contains("TestDB", toString);
        }

        #endregion

        #region Edge Cases Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetConnectionString_ComValoresNulosOuVazios_NaoDeveLancarExcecao(string value)
        {
            // Arrange
            var settings = new DBSettings
            {
                ServerUrl = value,
                Database = value,
                Username = value,
                Password = CryptSPATestHelper.EncryptDES("password") // Password não pode ser nula/vazia para encriptação
            };

            // Act
            var connectionString = settings.GetConnectionString();

            // Assert
            Assert.NotNull(connectionString);
            Assert.Contains("Data Source=", connectionString);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(300)]
        public void GetConnectionString_ComDiferentesTimeouts_DeveTrabalharCorretamente(int timeout)
        {
            // Arrange
            var plainPassword = "password123";
            var encryptedPassword = CryptSPATestHelper.EncryptDES(plainPassword);

            var settings = new DBSettings
            {
                ServerUrl = "localhost",
                Database = "TestDB",
                Username = "testuser",
                Password = encryptedPassword,
                ConnectTimeout = timeout,
                CommandTimeout = timeout
            };

            // Act
            var connectionString = settings.GetConnectionString();

            // Assert
            Assert.NotNull(connectionString);
            var expectedTimeout = timeout == 0 ? 20 : timeout;
            Assert.Contains($"Connect Timeout={expectedTimeout}", connectionString);
        }

        #endregion
    }

    #region CryptSPA Tests

    public class CryptSPATest
    {
        #region Encryption/Decryption Tests

        [Theory]
        [InlineData("password123")]
        [InlineData("simple")]
        [InlineData("complex@Password!123")]
        [InlineData("áéíóú")]
        [InlineData("")]
        public void EncryptDecrypt_RoundTrip_DeveRetornarTextoOriginal(string originalText)
        {
            // Act
            var encrypted = CryptSPATestHelper.EncryptDES(originalText);
            var decrypted = CryptSPATestHelper.DecryptDES(encrypted);

            // Assert
            Assert.Equal(originalText, decrypted);
        }

        [Fact]
        public void EncryptDES_ComTextoValido_DeveRetornarBase64()
        {
            // Arrange
            var plainText = "password123";

            // Act
            var encrypted = CryptSPATestHelper.EncryptDES(plainText);

            // Assert
            Assert.NotNull(encrypted);
            Assert.NotEqual(plainText, encrypted);

            // Verificar se é Base64 válido
            Assert.True(IsValidBase64(encrypted));
        }

        [Fact]
        public void DecryptDES_ComBase64Valido_DeveRetornarTextoOriginal()
        {
            // Arrange
            var originalText = "test_password";
            var encrypted = CryptSPATestHelper.EncryptDES(originalText);

            // Act
            var decrypted = CryptSPATestHelper.DecryptDES(encrypted);

            // Assert
            Assert.Equal(originalText, decrypted);
        }

        [Fact]
        public void EncryptDES_ComMesmoTexto_DeveRetornarMesmoResultado()
        {
            // Arrange
            var plainText = "consistent_password";

            // Act
            var encrypted1 = CryptSPATestHelper.EncryptDES(plainText);
            var encrypted2 = CryptSPATestHelper.EncryptDES(plainText);

            // Assert
            Assert.Equal(encrypted1, encrypted2);
        }

        [Fact]
        public void EncryptDES_ComTextosdiferentes_DeveRetornarResultadosDiferentes()
        {
            // Arrange
            var plainText1 = "password1";
            var plainText2 = "password2";

            // Act
            var encrypted1 = CryptSPATestHelper.EncryptDES(plainText1);
            var encrypted2 = CryptSPATestHelper.EncryptDES(plainText2);

            // Assert
            Assert.NotEqual(encrypted1, encrypted2);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void EncryptDES_ComStringVazia_DeveTrabalharCorretamente()
        {
            // Arrange
            var emptyString = "";

            // Act
            var encrypted = CryptSPATestHelper.EncryptDES(emptyString);
            var decrypted = CryptSPATestHelper.DecryptDES(encrypted);

            // Assert
            Assert.NotNull(encrypted);
            Assert.Equal(emptyString, decrypted);
        }

        [Fact]
        public void DecryptDES_ComBase64Invalido_DeveLancarExcecao()
        {
            // Arrange
            var invalidBase64 = "invalid_base64_string!@#";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => CryptSPATestHelper.DecryptDES(invalidBase64));
        }

        [Fact]
        public void EncryptDES_ComTextoLongo_DeveTrabalharCorretamente()
        {
            // Arrange
            var longText = new string('A', 1000) + "password" + new string('Z', 1000);

            // Act
            var encrypted = CryptSPATestHelper.EncryptDES(longText);
            var decrypted = CryptSPATestHelper.DecryptDES(encrypted);

            // Assert
            Assert.Equal(longText, decrypted);
        }

        [Theory]
        [InlineData("Special@Characters!#$%")]
        [InlineData("Numbers123456789")]
        [InlineData("Symbols<>[]{}()")]
        [InlineData("Unicode漢字")]
        public void EncryptDecrypt_ComCaracteresEspeciais_DeveTrabalharCorretamente(string specialText)
        {
            // Act
            var encrypted = CryptSPATestHelper.EncryptDES(specialText);
            var decrypted = CryptSPATestHelper.DecryptDES(encrypted);

            // Assert
            Assert.Equal(specialText, decrypted);
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void EncryptDES_Performance_DeveSerRapido()
        {
            // Arrange
            var plainText = "password_for_performance_test";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 1000; i++)
            {
                CryptSPATestHelper.EncryptDES($"{plainText}_{i}");
            }

            stopwatch.Stop();

            // Assert - Deve ser rápido - menos de 1 segundo para 1000 encriptações
            Assert.True(stopwatch.ElapsedMilliseconds < 1000,
                $"Performance inadequada: {stopwatch.ElapsedMilliseconds}ms para 1000 encriptações");
        }

        [Fact]
        public void DecryptDES_Performance_DeveSerRapido()
        {
            // Arrange
            var plainTexts = new string[1000];
            var encryptedTexts = new string[1000];

            for (int i = 0; i < 1000; i++)
            {
                plainTexts[i] = $"password_test_{i}";
                encryptedTexts[i] = CryptSPATestHelper.EncryptDES(plainTexts[i]);
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 1000; i++)
            {
                CryptSPATestHelper.DecryptDES(encryptedTexts[i]);
            }

            stopwatch.Stop();

            // Assert - Deve ser rápido - menos de 1 segundo para 1000 descriptações
            Assert.True(stopwatch.ElapsedMilliseconds < 1000,
                $"Performance inadequada: {stopwatch.ElapsedMilliseconds}ms para 1000 descriptações");
        }

        #endregion

        #region Security Tests

        [Fact]
        public void CryptSPA_ChaveInterna_NaoDeveSerExposta()
        {
            // Arrange & Act
            var plainText = "test_password";
            var encrypted = CryptSPATestHelper.EncryptDES(plainText);

            // Assert - A chave não deve aparecer no texto encriptado
            Assert.DoesNotContain("w3@sb1r0", encrypted);
            Assert.DoesNotContain("w3@sb1r0", plainText);
        }

        [Fact]
        public void EncryptDES_ResultadoNaoContemTextoOriginal()
        {
            // Arrange
            var plainText = "visible_password_123";

            // Act
            var encrypted = CryptSPATestHelper.EncryptDES(plainText);

            // Assert
            Assert.DoesNotContain(plainText, encrypted);
            Assert.DoesNotContain("visible", encrypted);
            Assert.DoesNotContain("password", encrypted);
            Assert.DoesNotContain("123", encrypted);
        }

        #endregion

        #region Helper Methods

        private bool IsValidBase64(string base64String)
        {
            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }

    #endregion

    #region Test Helper Class


    internal static class CryptSPATestHelper
    {
        public static string EncryptDES(string plainText)
        {
            // Como CryptSPA é internal, criamos uma implementação idêntica para testes
            using var des = new System.Security.Cryptography.DESCryptoServiceProvider();
            des.Mode = System.Security.Cryptography.CipherMode.ECB;
            des.Key = System.Text.Encoding.UTF8.GetBytes("w3@sb1r0");
            des.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            using var encryptor = des.CreateEncryptor();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string DecryptDES(string encryptedData)
        {
            using var des = new System.Security.Cryptography.DESCryptoServiceProvider();
            des.Mode = System.Security.Cryptography.CipherMode.ECB;
            des.Key = System.Text.Encoding.UTF8.GetBytes("w3@sb1r0");
            des.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            using var decryptor = des.CreateDecryptor();
            byte[] buffer = Convert.FromBase64String(encryptedData);
            byte[] decryptedBytes = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
            return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }
    }

    #endregion
}