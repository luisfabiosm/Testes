
using Domain.Core.Common.Serialization;
using System.Text.Json;

namespace Domain.Core.Exceptions
{
    public sealed class ErrorDetailsReturn
    {

        public int tipoErro { get; init; }
        public int codErro { get; init; }
        public string msgErro { get; init; }
        public string origemErro { get; set; }

          public List<ValidationErrorDetails>? ValidationErrors { get; init; }

       
        public static ErrorDetailsReturn Create(int tipo, int codigo, string mensagem, string origem = "")
        {
            return new ErrorDetailsReturn
            {

                tipoErro = tipo,
                codErro = codigo,
                msgErro = mensagem,
                origemErro = origem
            };
        }
      
        public static ErrorDetailsReturn Create(string result)
        {
            var _spsErro = ValidateReturnedErrorMessage(result);

            return ErrorDetailsReturn.Create(_spsErro.tipoErro, _spsErro.codErro, _spsErro.msgErro, _spsErro.origemErro);
        }

        public static ErrorDetailsReturn CreateWithValidationErrors(int tipo, int codigo, string mensagem, string origem, List<ValidationErrorDetails> validationErrors)
        {
            return new ErrorDetailsReturn
            {
                tipoErro = tipo,
                codErro = codigo,
                msgErro = mensagem,
                origemErro = origem,
                ValidationErrors = validationErrors
            };
        }

        private static ErrorDetailsReturn ValidateReturnedErrorMessage(string result)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(result))
                    throw new ValidateException("Mensagem de erro em formato invalido");

                var _spsErro = JsonSerializer.Deserialize<ErrorDetailsReturn>(result, JsonOptions.Default);
                return _spsErro;
            }

            catch (Exception ex)
            {
                throw new ValidateException("Mensagem de erro em formato invalido");

            }

        }



    }
}
