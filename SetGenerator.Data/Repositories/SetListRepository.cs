using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface ISetlistRepository : IRepositoryBase<Setlist>
    {
        Setlist GetByName(int bandId, string name);
    }

    public class SetlistRepository : RepositoryBase<Setlist>, ISetlistRepository
    {
        public SetlistRepository(ISession session)
            : base(session)
        {
        }

        public Setlist GetByName(int bandId, string name)
        {
            var sl = GetAll()
                .Where(x => x.Band.Id == bandId)
                .FirstOrDefault(x => x.Name == name);

            return sl;
        }
    }
}
