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
        IEnumerable<SongMemberInstrument> GetMemberInstrumentList(int id);
        IEnumerable<int> GetSongInstrumentIds();
        IEnumerable<int> GetSongGenreIds();

        void DeleteBandSongs(int bandId);
        void DeleteBandSetlistSongs(int bandId);

        // Key
        Key GetKey(int keyId);
        IEnumerable<Key> GetKeyListFull();
        IEnumerable<string> GetKeyNameList();
        ArrayList GetKeyNameArrayList();

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

        public IEnumerable<SongMemberInstrument> GetMemberInstrumentList(int id)
        {
            var list = Get(id)
                .SongMemberInstruments
                .ToList();

            return list;
        }

        public IEnumerable<int> GetSongInstrumentIds()
        {
            var list = Session.QueryOver<SongMemberInstrument>().List()
                .Select(x => x.Instrument.Id)
                .Distinct();

            return list;
        }

        public IEnumerable<int> GetSongGenreIds()
        {
            var list = Session.QueryOver<Song>().List()
                .Select(x => x.Genre.Id)
                .Distinct();

            return list;
        }

        public void DeleteBandSongs(int bandId)
        {
            var songs = Session.QueryOver<Song>()
                .Where(x => x.Band.Id == bandId)
                .List();

            foreach (var song in songs)
            {
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(song);
                    transaction.Commit();
                }
            }
        }

        public void DeleteBandSetlistSongs(int bandId)
        {
            DeleteBandSetSongs(bandId);
            DeleteBandSetlists(bandId);
        }

        private void DeleteBandSetlists(int bandId)
        {
            var setlists = Session.QueryOver<Setlist>()
                .Where(x => x.Band.Id == bandId)
                .List();

            foreach (var setlist in setlists)
            {
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(setlist);
                    transaction.Commit();
                }
            }
        }

        private void DeleteBandSetSongs(int bandId)
        {
            Member songTableAlias = null;

            var setSongs = Session.QueryOver<SetSong>()
                .JoinAlias(x => x.Song, () => songTableAlias)
                .Where(x => x.Song.Id == songTableAlias.Id)
                .Where(x => songTableAlias.Band.Id == bandId)
                .List();

            foreach (var setSong in setSongs)
            {
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(setSong);
                    transaction.Commit();
                }
            }
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

        // Tempo

        public IEnumerable<Tempo> GetAllTempos()
        {
            return Session.QueryOver<Tempo>()
                .List()
                .OrderBy(o => o.Name)
                .ToArray();
        }

        public Tempo GetTempo(int tempoId)
        {
            return Session.QueryOver<Tempo>()
                .Where(t => t.Id == tempoId)
                .SingleOrDefault();
        }

        public ArrayList GetTempoArrayList()
        {
            var tempos = Session.QueryOver<Tempo>()
                .List()
                .OrderBy(o => o.Name);

            var al = new ArrayList();

            foreach (var t in tempos)
                al.Add(new { Value = t.Id, Display = t.Name });

            return al;
        }
    }
}
