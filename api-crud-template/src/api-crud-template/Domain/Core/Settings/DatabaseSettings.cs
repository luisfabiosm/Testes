using System.Security.Cryptography;
using System.Text;

namespace Domain.Core.Settings
{
    public record DatabaseSettings
    {
        public int CommandTimeout { get; set; } = 30;
        public int ConnectTimeout { get; set; } = 30;
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);
        public string Cluster { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }


        public bool IsTraceExecActive { get; set; } = false;

        public string GetConnectionString()
        {
            var _ConnectTimeout = ConnectTimeout == 0 ? 10 : ConnectTimeout;
            return $"Data Source={Cluster};Initial Catalog={Database};Persist Security Info=True;User ID={Username};Password={CryptSPA.decryptDES(Password)};MultipleActiveResultSets=true;Connect Timeout={_ConnectTimeout};Enlist=false;TrustServerCertificate=True;";
           // return $"Data Source={Cluster};Initial Catalog={Database};Persist Security Info=True;User ID={Username};Password={CryptSPA.decryptDES(Password)};Connect Timeout={_ConnectTimeout}";
        }


        public string GetInfoNoPasswordConnectionString()
        {
            return $"Data Source={Cluster};Initial Catalog={Database};Persist Security Info=True;User ID={Username};Não apresenta o password aberto";
        }
    }

    internal static class CryptSPA
    {
        public static string decryptDES(string sDados)
        {
            string chave = "w3@sb1r0";

            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

            DES.Mode = CipherMode.ECB;
            DES.Key = Encoding.UTF8.GetBytes(chave);
            DES.Padding = PaddingMode.PKCS7;

            ICryptoTransform DESEncrypt = DES.CreateDecryptor();
            byte[] Buffer = Convert.FromBase64String(sDados);

            return Encoding.UTF8.GetString(DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
        }
    }
}