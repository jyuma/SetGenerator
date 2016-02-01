using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface ISongRepository : IRepositoryBase<Song>
    {
        Song GetByTitle(int bandId, string title);
        IEnumerable<Song> GetAList(int bandId);
        IEnumerable<Song> GetByBandId(int bandId);
        IEnumerable<string> GetComposerList(int bandId);
        IEnumerable<SongMemberInstrument> GetMemberInstrumentList(int songId);

        // Key
        Key GetKey(int keyId);
        IEnumerable<Key> GetKeyListFull();
        IEnumerable<string> GetKeyNameList();
        ArrayList GetKeyNameArrayList();

        // Genre
        IEnumerable<Genre> GetAllGenres();
        Genre GetGenre(int genreId);
        ArrayList GetGenreArrayList();

        // Tempo
        IEnumerable<Tempo> GetAllTempos();
        Tempo GetTempo(int tempoId);
        ArrayList GetTempoArrayList();
    }

    public class SongRepository : RepositoryBase<Song>, ISongRepository
    {
        public SongRepository(ISession session)
            : base(session)
        {
        }

        public IEnumerable<Song> GetAList(int bandId)
        {
            var list = GetByBandId(bandId)
                 .Where(x => x.Band.Id == bandId)
                 .Where(x => x.IsDisabled == false)
                 .OrderBy(x => x.Title)
                 .ToArray();

            return list;
        }

        public Song GetByTitle(int bandId, string title)
        {
            var song = GetByBandId(bandId)
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

        // Key

        public Key GetKey(int keyId)
        {
            return Session.QueryOver<Key>()
                .Where(k => k.Id == keyId)
                .SingleOrDefault();
        }

        public IEnumerable<Key> GetKeyListFull()
        {
            return Session.QueryOver<Key>()
                .List()
                .ToArray();
        }

        public IEnumerable<string> GetKeyNameList()
        {
            return Session.QueryOver<Key>()
                .List()
                .OrderBy(x => x.KeyName.Name)
                .Select(x => x.KeyName.Name)
                .Distinct()
            .ToArray();
        }

        public ArrayList GetKeyNameArrayList()
        {
            var keyNames = Session.QueryOver<Key>()
                 .List()
                 .OrderBy(x => x.KeyName.Name)
                 .Select(x => x.KeyName)
                 .Distinct()
                 .ToArray();

            var al = new ArrayList();

            foreach (var k in keyNames)
                al.Add(new { Value = k.Id, Display = k.Name });

            return al;
        }

        // Genre

        public IEnumerable<Genre> GetAllGenres()
        {
            return Session.QueryOver<Genre>()
                .List()
                .ToArray();
        }

        public Genre GetGenre(int genreId)
        {
            return Session.QueryOver<Genre>()
                .Where(k => k.Id == genreId)
                .SingleOrDefault();
        }

        public ArrayList GetGenreArrayList()
        {
            var genres = Session.QueryOver<Genre>()
                .List();

            var al = new ArrayList();

            foreach (var g in genres)
                al.Add(new { Value = g.Id, Display = g.Name });

            return al;
        }

        // Tempo

        public IEnumerable<Tempo> GetAllTempos()
        {
            return Session.QueryOver<Tempo>()
                .List()
                .ToArray();
        }

        public Tempo GetTempo(int tempoId)
        {
            return Session.QueryOver<Tempo>()
                .Where(k => k.Id == tempoId)
                .SingleOrDefault();
        }

        public ArrayList GetTempoArrayList()
        {
            var tempos = Session.QueryOver<Genre>()
                .List();

            var al = new ArrayList();

            foreach (var t in tempos)
                al.Add(new { Value = t.Id, Display = t.Name });

            return al;
        }
    }
}
