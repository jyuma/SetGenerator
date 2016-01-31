using System;

namespace SetGenerator.Domain.Entities
{
    public abstract class EntityAuditBase : EntityBase
    {
        public virtual User UserCreate { get; set; }
        public virtual User UserUpdate { get; set; }
        public virtual DateTime DateCreate { get; set; }
        public virtual DateTime DateUpdate { get; set; }
    }
}
