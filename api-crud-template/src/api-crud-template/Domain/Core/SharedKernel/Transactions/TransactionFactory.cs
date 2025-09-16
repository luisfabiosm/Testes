


using Domain.Core.Interfaces.Domain;
using Domain.Core.Models.Request;
using Domain.UseCases.AddUser;

namespace Domain.Core.SharedKernel.Transactions;

public class TransactionFactory : ITransactionFactory
{

    public TransactionFactory()
    {
    }

    public TransactionAddUser CreateAddUserTransaction(HttpContext context, AddNewUserRequest request, string correlationId)
    {
        return new TransactionAddUser
        {
            CorrelationId = correlationId,
            Code = 1,
            canal = GetCanal(context),
            NewUser = request.ToUser(),
            chaveIdempotencia = GetChaveIdempotencia(context)
        };
    }

    internal short GetCanal(HttpContext context)
    {
        var claim = context.User.FindFirst("Canal")?.Value;

        if (string.IsNullOrWhiteSpace(claim))
            throw new UnauthorizedAccessException("Claim obrigatória 'Canal' não encontrada.");

        if (!short.TryParse(claim, out var canal))
            throw new FormatException("Claim 'Canal' inválida.");

        return canal;
    }

    internal string GetChaveIdempotencia(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Chave-Idempotencia", out var chave) || string.IsNullOrWhiteSpace(chave))
            throw new ArgumentException("Cabeçalho obrigatório 'Chave-Idempotencia' não encontrado ou vazio.");

        return chave.ToString();
    }
}

