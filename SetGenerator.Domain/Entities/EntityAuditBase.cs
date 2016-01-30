using System;

namespace SetGenerator.Domain.Entities
{
    public abstract class EntityAuditBase : EntityBase
    {
        public virtual int UserCreateId { get; set; }
        public virtual int UserUpdateId { get; set; }
        public virtual DateTime DateCreate { get; set; }
        public virtual DateTime DateUpdate { get; set; }
    }
}
