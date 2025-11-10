using System;

namespace Biro.Core.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        public string Code { get; }

        protected DomainException(string code, string message) : base(message)
        {
            Code = code;
        }

        protected DomainException(string code, string message, Exception innerException) 
            : base(message, innerException)
        {
            Code = code;
        }
    }

    public class InsufficientBalanceException : DomainException
    {
        public decimal RequestedAmount { get; }
        public decimal AvailableBalance { get; }

        public InsufficientBalanceException(decimal requestedAmount, decimal availableBalance)
            : base("INSUFFICIENT_BALANCE", 
                  $"Insufficient balance. Requested: {requestedAmount:C}, Available: {availableBalance:C}")
        {
            RequestedAmount = requestedAmount;
            AvailableBalance = availableBalance;
        }
    }

    public class AccountNotFoundException : DomainException
    {
        public Guid AccountId { get; }

        public AccountNotFoundException(Guid accountId)
            : base("ACCOUNT_NOT_FOUND", $"Account with ID {accountId} not found")
        {
            AccountId = accountId;
        }
    }

    public class ClientNotFoundException : DomainException
    {
        public Guid ClientId { get; }

        public ClientNotFoundException(Guid clientId)
            : base("CLIENT_NOT_FOUND", $"Client with ID {clientId} not found")
        {
            ClientId = clientId;
        }
    }

    public class TransactionNotFoundException : DomainException
    {
        public Guid TransactionId { get; }

        public TransactionNotFoundException(Guid transactionId)
            : base("TRANSACTION_NOT_FOUND", $"Transaction with ID {transactionId} not found")
        {
            TransactionId = transactionId;
        }
    }

    public class InvalidTransactionException : DomainException
    {
        public InvalidTransactionException(string message)
            : base("INVALID_TRANSACTION", message)
        {
        }
    }

    public class AccountNotActiveException : DomainException
    {
        public Guid AccountId { get; }
        public string Status { get; }

        public AccountNotActiveException(Guid accountId, string status)
            : base("ACCOUNT_NOT_ACTIVE", 
                  $"Account {accountId} is not active. Current status: {status}")
        {
            AccountId = accountId;
            Status = status;
        }
    }

    public class DuplicateAccountException : DomainException
    {
        public string AccountNumber { get; }

        public DuplicateAccountException(string accountNumber)
            : base("DUPLICATE_ACCOUNT", 
                  $"An account with number {accountNumber} already exists")
        {
            AccountNumber = accountNumber;
        }
    }

    public class TransactionLimitExceededException : DomainException
    {
        public decimal Amount { get; }
        public decimal Limit { get; }
        public string LimitType { get; }

        public TransactionLimitExceededException(decimal amount, decimal limit, string limitType)
            : base("TRANSACTION_LIMIT_EXCEEDED", 
                  $"Transaction amount {amount:C} exceeds {limitType} limit of {limit:C}")
        {
            Amount = amount;
            Limit = limit;
            LimitType = limitType;
        }
    }

    public class InvalidAccountOperationException : DomainException
    {
        public InvalidAccountOperationException(string operation, string reason)
            : base("INVALID_ACCOUNT_OPERATION", 
                  $"Cannot perform operation '{operation}': {reason}")
        {
        }
    }

    public class TransactionAlreadyProcessedException : DomainException
    {
        public Guid TransactionId { get; }

        public TransactionAlreadyProcessedException(Guid transactionId)
            : base("TRANSACTION_ALREADY_PROCESSED", 
                  $"Transaction {transactionId} has already been processed")
        {
            TransactionId = transactionId;
        }
    }
}
