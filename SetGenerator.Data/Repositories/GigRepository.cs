using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IGigRepository : IRepositoryBase<Gig>
    {
        IEnumerable<Gig> GetByBandId(int bandId);
    }

    public class GigRepository : RepositoryBase<Gig>, IGigRepository
    {
        public GigRepository(ISession session)
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

    }
}
