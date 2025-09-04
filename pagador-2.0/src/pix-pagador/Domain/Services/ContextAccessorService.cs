namespace Domain.Services
{
    public class ContextAccessorService
    {
        public short GetCanal(HttpContext context)
        {
            var claim = context.User.FindFirst("Canal")?.Value;

            if (string.IsNullOrWhiteSpace(claim))
                throw new UnauthorizedAccessException("Claim obrigatória 'Canal' não encontrada.");

            if (!short.TryParse(claim, out var canal))
                throw new FormatException("Claim 'Canal' inválida.");

            return canal;
        }

        public string GetChaveIdempotencia(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("Chave-Idempotencia", out var chave) || string.IsNullOrWhiteSpace(chave))
                throw new ArgumentException("Cabeçalho obrigatório 'Chave-Idempotencia' não encontrado ou vazio.");

            return chave.ToString();
        }
    }
}
