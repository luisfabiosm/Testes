using System;

namespace Biro.Core.Domain.Entities
{
    public abstract record Entity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public bool IsDeleted { get; protected set; }
        public string CreatedBy { get; protected set; }
        public string UpdatedBy { get; protected set; }
        public byte[] RowVersion { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }

        public virtual void Delete()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Restore()
        {
            IsDeleted = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAuditInfo(string userId, bool isUpdate = false)
        {
            if (isUpdate)
            {
                UpdatedBy = userId;
                UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                CreatedBy = userId;
            }
        }
    }
}