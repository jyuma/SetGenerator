﻿using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class SetlistMap : ClassMap<Setlist>
    {
        public SetlistMap()
        {
            Table("Setlist");

            Id(x => x.Id).Column("Id");
            Map(m => m.Name).Column("Name");
            References(m => m.Band, "BandId").Column("Id");
            References(m => m.UserCreate).Column("UserCreateId");
            References(m => m.UserUpdate).Column("UserUpdateId");

            HasMany(m => m.Sets);
        }
    }
}