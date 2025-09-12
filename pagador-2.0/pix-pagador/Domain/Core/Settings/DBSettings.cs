using System.Security.Cryptography;
using System.Text;

namespace Domain.Core.Settings
{
    public record DBSettings
    {
        public string ServerUrl { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int CommandTimeout { get; set; } = 10;
        public int ConnectTimeout { get; set; } = 10;
        public int Port { get; set; }

        public DBSettings()
        {
            
        }
        public string GetConnectionString()
        {
            var _ConnectTimeout = ConnectTimeout == 0 ? 20 : ConnectTimeout;

            return $"Data Source={ServerUrl};Initial Catalog={Database};TrustServerCertificate=True;Persist Security Info=True;User ID={Username};Password={CryptSPA.decryptDES(Password)};MultipleActiveResultSets=true;Connect Timeout={_ConnectTimeout};Enlist=false";

        }

        public string GetConnectionNoCryptString()
        {
            var _ConnectTimeout = ConnectTimeout == 0 ? 20 : ConnectTimeout;

            return $"Data Source={ServerUrl};Initial Catalog={Database};TrustServerCertificate=True;Persist Security Info=True;User ID={Username};Password={Password};MultipleActiveResultSets=true;Connect Timeout={_ConnectTimeout};Enlist=false";

        }

    }

    internal static class CryptSPA
    {
        static string chave = "w3@sb1r0";

        public static string decryptDES(string sDados)
        {

            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

            DES.Mode = CipherMode.ECB;
            DES.Key = Encoding.UTF8.GetBytes(chave);
            DES.Padding = PaddingMode.PKCS7;

            ICryptoTransform DESEncrypt = DES.CreateDecryptor();
            byte[] Buffer = Convert.FromBase64String(sDados);

            return Encoding.UTF8.GetString(DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
        }

        public static string encryptDES(string sDados)
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

            DES.Mode = CipherMode.ECB;
            DES.Key = Encoding.UTF8.GetBytes(chave);
            DES.Padding = PaddingMode.PKCS7;
            ICryptoTransform DESEncrypt = DES.CreateEncryptor();

            // Convert the input string to bytes
            byte[] inputBytes = Encoding.UTF8.GetBytes(sDados);

            // Encrypt the bytes
            byte[] encryptedBytes = DESEncrypt.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            // Convert the encrypted bytes to base64 string
            return Convert.ToBase64String(encryptedBytes);
        }
    }
}