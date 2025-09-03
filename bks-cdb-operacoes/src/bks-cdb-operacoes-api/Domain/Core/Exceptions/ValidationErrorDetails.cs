namespace Domain.Core.Exceptions
{
    public record ValidationErrorDetails
    {
        public string mensagem { get; set; }
        public string campo { get; set; }

        public ValidationErrorDetails(string propertyName, string message)
        {
            mensagem = message;
            campo = propertyName;
        }

    }
}
