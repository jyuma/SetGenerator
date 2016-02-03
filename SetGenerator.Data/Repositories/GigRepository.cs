using System;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IGigRepository : IRepositoryBase<Gig>
    {
        IEnumerable<Gig> GetByBandId(int bandId);
        Gig GetByBandVenueDateGig(int bandId, string venue, DateTime dateGig);
        IEnumerable<string> GetVenueList(int bandId);
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

        public Gig GetByBandVenueDateGig(int bandId, string venue, DateTime dateGig)
        {
            var gig = Session.QueryOver<Gig>()
                .Where(x => x.Band.Id == bandId)
                .Where(x => x.Venue == venue)
                .Where(x => x.DateGig == dateGig)
                .SingleOrDefault();

            return gig;
        }

        public IEnumerable<string> GetVenueList(int bandId)
        {
            var list = Session.QueryOver<Gig>()
                .Where(x => x.Band.Id == bandId)
                .Where(x => x.Venue != null)
                .List()
                .OrderBy(x => x.Venue)
                .Select(x => x.Venue)
                .Distinct()
                .ToArray();

            return list;
        }
    }
}
