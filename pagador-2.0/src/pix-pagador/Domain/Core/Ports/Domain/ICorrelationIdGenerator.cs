namespace Domain.Core.Ports.Domain
{
    public interface ICorrelationIdGenerator
    {
        string Generate(int length = 16);

        string GenerateWithPrefix(string prefix, int idLength = 16);

        bool IsValid(string correlationId);
    }
}
