using System.Text.Json.Serialization;

namespace Domain.Core.Base
{
    public record BaseError
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? code { get; set; }
        public string? message { get; set; }
        public string? info { get; set; }
        public BaseError() { }

        public BaseError(string codigo, string mensagem, string? info = null)
        {
            code = codigo;
            message = mensagem;
            this.info = info;
        }

        ~BaseError()
        {
            code = null;
            message = null;
            info = null;
        }
    }
}
