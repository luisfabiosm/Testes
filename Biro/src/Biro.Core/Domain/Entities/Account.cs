

using Biro.Core.Domain.Enums;

namespace Biro.Core.Domain.Entities
{
    public record Account : Entity
    {
        public Guid ClientId { get; private set; }
        public string AccountNumber { get; private set; }
        public string BranchCode { get; private set; }
        public ProductType ProductType { get; private set; }
        public AccountStatus Status { get; private set; }
        public string Currency { get; private set; }
        public decimal DailyTransferLimit { get; private set; }
        public decimal MonthlyTransferLimit { get; private set; }
        public DateTime? ActivatedAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }

        private readonly List<Transaction> _transactions = new();
        public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

        public Account(
            Guid clientId,
            string branchCode,
            ProductType productType,
            string currency = "BRL")
        {
            ClientId = clientId;
            BranchCode = branchCode;
            ProductType = productType;
            Currency = currency;
            AccountNumber = GenerateAccountNumber();
            Status = AccountStatus.Pending;
            DailyTransferLimit = productType == ProductType.CheckingAccount ? 50000m : 10000m;
            MonthlyTransferLimit = productType == ProductType.CheckingAccount ? 500000m : 100000m;
        }

        public decimal GetBalance()
        {
            var credits = _transactions
                .Where(t => t.TransactionType == TransactionType.Credit ||
                           t.TransactionType == TransactionType.InitialBalance)
                .Sum(t => t.Amount);

            var debits = _transactions
                .Where(t => t.TransactionType == TransactionType.Debit ||
                           t.TransactionType == TransactionType.Block ||
                           t.TransactionType == TransactionType.Reservation)
                .Sum(t => t.Amount);

            return credits - debits;
        }

        public decimal GetAvailableBalance()
        {
            var balance = GetBalance();
            var blockedAmount = GetBlockedAmount();
            var reservedAmount = GetReservedAmount();

            return balance + blockedAmount + reservedAmount;
        }

        public decimal GetBlockedAmount()
        {
            return _transactions
                .Where(t => t.TransactionType == TransactionType.Block &&
                           t.Status == TransactionStatus.Completed)
                .Sum(t => t.Amount);
        }

        public decimal GetReservedAmount()
        {
            return _transactions
                .Where(t => t.TransactionType == TransactionType.Reservation &&
                           t.Status == TransactionStatus.Pending)
                .Sum(t => t.Amount);
        }

        public void AddTransaction(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (transaction.AccountId != Id)
                throw new InvalidOperationException("Transaction does not belong to this account");

            _transactions.Add(transaction);
            UpdatedAt = DateTime.UtcNow;
        }

        public IEnumerable<Transaction> GetTransactionsByType(TransactionType type)
        {
            return _transactions.Where(t => t.TransactionType == type);
        }

        public IEnumerable<Transaction> GetTransactionsByPeriod(DateTime startDate, DateTime endDate)
        {
            return _transactions.Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate);
        }

        public void Activate()
        {
            if (Status != AccountStatus.Pending)
                throw new InvalidOperationException("Only pending accounts can be activated");

            Status = AccountStatus.Active;
            ActivatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Suspend()
        {
            if (Status == AccountStatus.Closed)
                throw new InvalidOperationException("Cannot suspend closed account");

            Status = AccountStatus.Suspended;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Block()
        {
            if (Status == AccountStatus.Closed)
                throw new InvalidOperationException("Cannot block closed account");

            Status = AccountStatus.Blocked;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Close()
        {
            if (GetBalance() != 0)
                throw new InvalidOperationException("Cannot close account with non-zero balance");

            Status = AccountStatus.Closed;
            ClosedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        private string GenerateAccountNumber()
        {
            var random = new Random();
            var checkDigit = random.Next(0, 9);
            return $"{DateTime.Now:yyyyMMdd}{random.Next(10000, 99999)}-{checkDigit}";
        }
    }
}