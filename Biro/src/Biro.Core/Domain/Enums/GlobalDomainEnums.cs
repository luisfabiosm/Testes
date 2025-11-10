using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biro.Core.Domain.Enums
{
    public enum DocumentType
    {
        CPF,
        CNPJ,
        Passport,
        RG
    }
    public enum ProductType
    {
        CheckingAccount,
        SavingsAccount
    }

    public enum TransactionType
    {
        None,
        Debit,
        Credit,
        Block,
        Reservation,
        InitialBalance
    }

    public enum TransactionStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Reversed,
        Cancelled
    }

    public enum AccountStatus
    {
        Pending,
        Active,
        Suspended,
        Blocked,
        Closed
    }

    public enum ClientStatus
    {
        Pending,
        Active,
        Suspended,
        Blocked,
        Inactive
    }

    public enum ClientType
    {
        Individual,
        Business
    }
}
