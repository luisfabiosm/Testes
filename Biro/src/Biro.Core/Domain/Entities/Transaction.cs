using Biro.Core.Domain.Enums;
using System;

namespace Biro.Core.Domain.Entities
{
    public record Transaction : Entity
    {
        public Guid AccountId { get; private set; }
        public TransactionType TransactionType { get; private set; }
        public decimal Amount { get; private set; }
        public TransactionStatus Status { get; private set; }
        public string Description { get; private set; }
        public string ReferenceNumber { get; private set; }
        public Guid? RelatedTransactionId { get; private set; }
        public string ExternalId { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public DateTime? ReversedAt { get; private set; }
        public string ReversalReason { get; private set; }
        public TransactionMetadata Metadata { get; private set; }

        public Transaction(
            Guid accountId,
            TransactionType transactionType,
            decimal amount,
            string description = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            AccountId = accountId;
            TransactionType = transactionType;
            Amount = amount;
            Description = description ?? GetDefaultDescription(transactionType);
            ReferenceNumber = GenerateReferenceNumber();
            Status = TransactionStatus.Pending;
            Metadata = new TransactionMetadata();
        }

        public void Process()
        {
            if (Status != TransactionStatus.Pending)
                throw new InvalidOperationException("Only pending transactions can be processed");

            Status = TransactionStatus.Processing;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            if (Status != TransactionStatus.Processing)
                throw new InvalidOperationException("Only processing transactions can be completed");

            Status = TransactionStatus.Completed;
            ProcessedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Fail(string reason = null)
        {
            if (Status == TransactionStatus.Completed || Status == TransactionStatus.Reversed)
                throw new InvalidOperationException("Cannot fail completed or reversed transaction");

            Status = TransactionStatus.Failed;
            if (!string.IsNullOrWhiteSpace(reason))
            {
                Metadata.FailureReason = reason;
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reverse(string reason, Guid reversalTransactionId)
        {
            if (Status != TransactionStatus.Completed)
                throw new InvalidOperationException("Only completed transactions can be reversed");

            if (TransactionType == TransactionType.InitialBalance)
                throw new InvalidOperationException("Initial balance transactions cannot be reversed");

            Status = TransactionStatus.Reversed;
            ReversedAt = DateTime.UtcNow;
            ReversalReason = reason;
            RelatedTransactionId = reversalTransactionId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status != TransactionStatus.Pending)
                throw new InvalidOperationException("Only pending transactions can be cancelled");

            Status = TransactionStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetExternalId(string externalId)
        {
            ExternalId = externalId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateMetadata(Action<TransactionMetadata> updateAction)
        {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            updateAction(Metadata);
            UpdatedAt = DateTime.UtcNow;
        }

        private string GenerateReferenceNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            return $"{TransactionType.ToString().Substring(0, 3).ToUpper()}{timestamp}{random}";
        }

        private string GetDefaultDescription(TransactionType type)
        {
            return type switch
            {
                TransactionType.Credit => "Credit Transaction",
                TransactionType.Debit => "Debit Transaction",
                TransactionType.Block => "Amount Blocked",
                TransactionType.Reservation => "Amount Reserved",
                TransactionType.InitialBalance => "Initial Balance",
                _ => "Transaction"
            };
        }
    }

    public class TransactionMetadata
    {
        public string SourceBranch { get; set; }
        public string DestinationBranch { get; set; }
        public string SourceAccount { get; set; }
        public string DestinationAccount { get; set; }
        public string Channel { get; set; }
        public string DeviceId { get; set; }
        public string IpAddress { get; set; }
        public string Location { get; set; }
        public string FailureReason { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Notes { get; set; }
    }
}