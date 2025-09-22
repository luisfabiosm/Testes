


using Domain.Core.Interfaces.Domain;
using Domain.Core.Models.Request;
using Domain.UseCases.CreateUser;
using System.Security.Claims;

namespace Domain.Core.SharedKernel.Transactions;

public class TransactionFactory : ITransactionFactory
{

    public TransactionFactory()
    {
    }

    public TransactionCreateUser CreateUserTransaction(HttpContext context, CreateUserRequest request, string correlationId)
    {
        return new TransactionCreateUser
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
        if (Global.ENVIRONMENT == "Mock")
            return 1;

      
        var canalValue = context.User.FindFirst("Canal")?.Value ??
                         context.Request.Headers["Canal"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(canalValue))
            throw new ArgumentException("Canal não encontrado no Claim ou Header.");

        if (!short.TryParse(canalValue, out var canal))
            throw new FormatException("Canal deve ser um número válido.");

        return canal;
    }

    internal string GetChaveIdempotencia(HttpContext context)
    {
        if (Global.ENVIRONMENT == "Mock" )   
            return "teste";


        if (!context.Request.Headers.TryGetValue("Chave-Idempotencia", out var chave) || string.IsNullOrWhiteSpace(chave))
            throw new ArgumentException("Cabeçalho obrigatório 'Chave-Idempotencia' não encontrado ou vazio.");

        return chave.ToString();
    }
}

