using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class GenreMap : ClassMap<Genre>
    {
        public GenreMap()
        {
            Table("Genre");

            Id(x => x.Id).Column("Id");
            Map(m => m.Name).Column("Name");        
        }
    }
}
