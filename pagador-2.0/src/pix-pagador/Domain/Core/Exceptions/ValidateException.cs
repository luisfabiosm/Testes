using Domain.Core.Common.Serialization;
using Domain.Core.Enum;
using System.Text.Json;

namespace Domain.Core.Exceptions
{

    public class ValidateException : Exception
    {
        public int ErrorCode { get; internal set; } = -1;

        public List<ErrorDetails> RequestErrors { get; private set; }

        public ValidateException()
        {

        }
    

        public ValidateException(string message) : base(message)
        {
            
        }

        //public ValidateException(string message, int errorCode, object details)
        //   : base(message)
        //{
        //    this.ErrorCode = errorCode == -1 ? 400 : errorCode;
        //    erros = (List<ErrorDetails>)details;
        //}



        public static ValidateException Create(string mensagem, int codigo, List<ErrorDetails> details, string origem = "API")
        {
            var _msgErro = details.ToJsonOptimized(JsonOptions.Default);
            var _exception = new ValidateException(_msgErro);
            _exception.RequestErrors = details;
            return _exception;
        }
   


        public static ValidateException Create(List<ErrorDetails> details)
        {
            var _msgErro = JsonSerializer.Serialize(details, JsonOptions.Default); //details.ToJsonOptimized(JsonOptions.Default);
            var _exception = new ValidateException(_msgErro);
            _exception.RequestErrors = details;
            return _exception;
        }

    }
}