using Biro.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biro.Core.Domain.Entities
{
    public record Client : Entity
    {
        public string DocumentNumber { get; private set; }
        public string DocumentType { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public DateTime DateOfBirth { get; private set; }
        public ClientStatus Status { get; private set; }
        public ClientType Type { get; private set; }
        public DateTime? OnboardingCompletedAt { get; private set; }

        private readonly List<Account> _accounts = new();
        public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

        public Client(
            string documentNumber,
            string documentType,
            string name,
            string email,
            string phone,
            DateTime dateOfBirth,
            ClientType type = ClientType.Individual)
        {
            DocumentNumber = documentNumber;
            DocumentType = documentType;
            Name = name;
            Email = email;
            Phone = phone;
            DateOfBirth = dateOfBirth;
            Type = type;
            Status = ClientStatus.Pending;
        }

        public void AddAccount(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (_accounts.Any(a => a.Id == account.Id))
                throw new InvalidOperationException("Account already exists for this client");

            _accounts.Add(account);
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveAccount(Guid accountId)
        {
            var account = _accounts.FirstOrDefault(a => a.Id == accountId);
            if (account != null)
            {
                _accounts.Remove(account);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public Account GetAccount(Guid accountId)
        {
            return _accounts.FirstOrDefault(a => a.Id == accountId);
        }

        public IEnumerable<Account> GetAccountsByType(ProductType productType)
        {
            return _accounts.Where(a => a.ProductType == productType);
        }

        public void Activate()
        {
            if (Status == ClientStatus.Blocked)
                throw new InvalidOperationException("Cannot activate blocked client");

            Status = ClientStatus.Active;
            OnboardingCompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Suspend()
        {
            Status = ClientStatus.Suspended;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Block()
        {
            Status = ClientStatus.Blocked;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}