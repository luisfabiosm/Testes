namespace Domain.Core.Exceptions
{
    public record ErrorDetails
    {
        public string mensagens { get; set; }
        public string campo { get; set; }

        public ErrorDetails(string propertyName, string message)
        {
            mensagens = message;
            campo = propertyName;
        }


    }
}
