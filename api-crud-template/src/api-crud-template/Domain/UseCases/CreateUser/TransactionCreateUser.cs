using Domain.Core.Models.Entities;
using Domain.Core.SharedKernel.Transactions;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Drawing;
using System.Security.Cryptography.Xml;

namespace Domain.UseCases.CreateUser
{
    public record TransactionCreateUser : BaseTransaction
    {
        public required User NewUser { get; set; }


        public override string getTransactionSerialization()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
}
