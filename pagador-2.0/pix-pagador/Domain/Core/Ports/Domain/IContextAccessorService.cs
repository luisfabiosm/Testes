namespace Domain.Core.Ports.Domain
{
    public interface IContextAccessorService
    {
        short GetCanal(HttpContext context);
        string GetChaveIdempotencia(HttpContext context);
    }
}
