using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface ISetlistRepository : IRepositoryBase<Setlist>
    {
        IEnumerable<Gig> GetByBandId(int bandId);
        Setlist GetByName(int bandId, string name);
    }

    public class SetlistRepository : RepositoryBase<Setlist>, ISetlistRepository
    {
        public SetlistRepository(ISession session)
            : base(session)
        {
        }

        public IEnumerable<Gig> GetByBandId(int bandId)
        {
            var list = Session.QueryOver<Gig>()
                .Where(x => x.Band.Id == bandId)
                .List()
                .OrderBy(x => x.Venue);

            return list;
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
