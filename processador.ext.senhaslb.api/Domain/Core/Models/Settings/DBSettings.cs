using System.Security.Cryptography;
using System.Text;

namespace Domain.Core.Models.Settings
{
    public record DBSettings
    {
        public string? Cluster { get; set; }
        public string? Database { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int CommandTimeout { get; set; } = 30;
        public int ConnectTimeout { get; set; } = 10;
        public int ConnectionPool { get; init; }
        public bool IsTraceExecActive { get; set; } = false;

        public string GetConnectionString()
        {
            var _ConnectTimeout = this.ConnectTimeout == 0 ? 10 : this.ConnectTimeout;
            return $"Data Source={Cluster};Initial Catalog={Database};Persist Security Info=True;User ID={Username};Password={CryptSPA.decryptDES(Password!)};MultipleActiveResultSets=true;Connect Timeout={_ConnectTimeout};Enlist=false";
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

            #pragma warning disable S5547
            using (DES des = DES.Create())
            {
                des.Mode = CipherMode.ECB;
                des.Key = Encoding.UTF8.GetBytes(chave);
                des.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = des.CreateDecryptor())
                {
                    byte[] buffer = Convert.FromBase64String(sDados);
                    return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(buffer, 0, buffer.Length));
                }
            }
            #pragma warning restore S5547
        }
    }
}