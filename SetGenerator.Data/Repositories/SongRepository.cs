using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface ISongRepository : IRepositoryBase<Song>
    {
        IList<Song> GetAList(int bandId);
        Song GetByTitle(int bandId, string title);
        IEnumerable<Song> GetByBandId(int bandId);
        IEnumerable<string> GetComposerList(int bandId);
        IEnumerable<SongMemberInstrument> GetMemberInstrumentList(int songId);
    }

    public class SongRepository : RepositoryBase<Song>, ISongRepository
    {
        public SongRepository(ISession session)
            : base(session)
        {
        }

        public IList<Song> GetAList(int bandId)
        {
            var list = GetAll()
                 //.Include("Key")
                 //.Include("Key.KeyName")
                 //.Include("User")
                 //.Include("User1")
                 //.Include("Song")
                 //.Include("Tempo")
                 //.Include("SongMemberInstruments")
                 //.Include("SongMemberInstruments.Instrument")
                 .Where(x => x.Band.Id == bandId)
                 .Where(x => x.IsDisabled == false)
                 .OrderBy(x => x.Title)
                 .ToList();
            return list;
        }

        public Song GetByTitle(int bandId, string title)
        {
            var song = GetAll()
                .Where(x => x.Band.Id == bandId)
                .FirstOrDefault(x => x.Title == title);

            return song;
        }

        public IEnumerable<Song> GetByBandId(int bandId)
        {
            var list = Session.QueryOver<Song>()
                .Where(x => x.Band.Id == bandId)
                .List()
                .OrderBy(x => x.Title);

            return list;
        }

        public IEnumerable<string> GetComposerList(int bandId)
        {
            var list = Session.QueryOver<Song>()
                .Where(x => x.Band.Id == bandId)
                .Where(x => x.Composer != null)
                .List()
                .OrderBy(x => x.Composer)
                .Select(x => x.Composer)
                .Distinct()
                .ToArray();

            return list;
        }

        public IEnumerable<SongMemberInstrument> GetMemberInstrumentList(int songId)
        {
            var list = Get(songId)
                .SongMemberInstruments
                .ToList();

            return list;
        }
    }
}
