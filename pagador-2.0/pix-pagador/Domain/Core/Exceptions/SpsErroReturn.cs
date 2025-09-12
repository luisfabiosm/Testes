
using Domain.Core.Common.Serialization;
using System.Text.Json;

namespace Domain.Core.Exceptions
{
    public sealed class SpsErroReturn
    {

        public int tipoErro { get; init; }
        public int codErro { get; init; }
        public string msgErro { get; init; }
        public string origemErro { get; set; }


        public static SpsErroReturn Create(int tipo, int codigo, string mensagem, string origem = "")
        {
            return new SpsErroReturn
            {

                tipoErro = tipo,
                codErro = codigo,
                msgErro = mensagem,
                origemErro = origem
            };
        }
        public static SpsErroReturn Create(string spsReturn)
        {
            var _spsErro = ValidateReturnedErrorMessage(spsReturn);

            return SpsErroReturn.Create(_spsErro.tipoErro, _spsErro.codErro, _spsErro.msgErro, _spsErro.origemErro);
        }


        private static SpsErroReturn ValidateReturnedErrorMessage(string spsReturn)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(spsReturn))
                    throw new ValidateException("Mensagem de erro em formato invalido");

                var _spsErro = JsonSerializer.Deserialize<SpsErroReturn>(spsReturn, JsonOptions.Default);
                return _spsErro;
            }

            catch (Exception ex)
            {
                throw new ValidateException("Mensagem de erro em formato invalido");

            }

        }



    }
}
