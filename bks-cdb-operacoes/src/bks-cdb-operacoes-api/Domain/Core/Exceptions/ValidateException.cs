namespace Domain.Core.Exceptions
{

    public class ValidateException : Exception
    {
        public int ErrorCode { get; internal set; } = -1;

        public List<ValidationErrorDetails> RequestErrors { get; private set; }

        public ValidateException()
        {

        }
    
        public ValidateException(string message) : base(message)
        {
            
        }


        public static ValidateException Create(List<ValidationErrorDetails> details)
        {
            var errorMessage = $"Validação falhou com {details.Count} erro(s)";
            var _exception = new ValidateException(errorMessage);
            _exception.RequestErrors = details;
            return _exception;
        }

    }
}