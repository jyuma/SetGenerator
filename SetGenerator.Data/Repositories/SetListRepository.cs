﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface ISetlistRepository : IRepositoryBase<Setlist>
    {
        IEnumerable<Setlist> GetByBandId(int bandId);
        Setlist GetByName(int bandId, string name);
        ArrayList GetSetlistArrayList(int bandId);
    }

    public class SetlistRepository : RepositoryBase<Setlist>, ISetlistRepository
    {
        public SetlistRepository(ISession session)
            : base(session)
        {
        }

        public IEnumerable<Setlist> GetByBandId(int bandId)
        {
            var list = Session.QueryOver<Setlist>()
                .Where(x => x.Band.Id == bandId)
                .List()
                .OrderBy(x => x.Name);

            return list;
        }

        public Setlist GetByName(int bandId, string name)
        {
            var sl = GetAll()
                .Where(x => x.Band.Id == bandId)
                .FirstOrDefault(x => x.Name == name);

            return sl;
        }

        public ArrayList GetSetlistArrayList(int bandId)
        {
            var al = new ArrayList();

            var setlists = Session.QueryOver<Setlist>()
                .Where(x => x.Band.Id == bandId).List();

            foreach (var sl in setlists)
                al.Add(new { Value = sl.Id, Display = sl.Name });

            return al;
        }
    }
}
