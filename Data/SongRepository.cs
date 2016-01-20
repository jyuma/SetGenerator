using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain;

namespace SetGenerator.Repositories
{
    public interface ISongRepository
    {

        // database routines
        int Add(Song s);
        void Delete(Song s);
        void Update();

        Song GetSingle(int id);
        Song GetSingleForDelete(int id);
        Song GetByTitle(int bandId, string title);
        IEnumerable<Song> GetList(int bandId);
        IList<Song> GetAList(int bandId);
        IEnumerable<string> GetComposerList(int bandId);

        IEnumerable<SongMemberInstrument> GetMemberInstrumentList(int bandId);
    }

    public class SongRepository : ISongRepository
    {
        private readonly SetGeneratorEntities _context;

        public SongRepository(SetGeneratorEntities context)
        {
            _context = context;
            //_context.Configuration.LazyLoadingEnabled = false;
        }

        public Song GetSingle(int id)
        {
            var s = _context.Songs
                .Include("Key")
                .Include("Key.KeyName")
                .Include("User1")
                .Include("SongMemberInstruments")
                .FirstOrDefault(x => x.Id == id);
            return s;
        }

        public Song GetSingleForDelete(int id)
        {
            var s = _context.Songs
                .Include("SongMemberInstruments")
                .FirstOrDefault(x => x.Id == id);
            return s;
        }

        public IEnumerable<Song> GetList(int bandId)
        {
           var list = _context.Songs
                .Include("Key")
                .Include("Key.KeyName")
                .Include("User1")
                .Include("SongMemberInstruments")
                .Include("SongMemberInstruments.Instrument")
                .Where(x => x.BandId == bandId)
                .OrderBy(x => x.Title)
                .ToList();
            return list;
        }

        public IList<Song> GetAList(int bandId)
        {
            var list = _context.Songs
                 .Include("Key")
                 .Include("Key.KeyName")
                 .Include("User")
                 .Include("User1")
                 .Include("SongMemberInstruments")
                 .Include("SongMemberInstruments.Instrument")
                 .Where(x => x.BandId == bandId)
                 .Where(x => x.IsDisabled == false)
                 .OrderBy(x => x.Title)
                 .ToList();
            return list;
        }

        public Song GetByTitle(int bandId, string title)
        {
            var s = _context.Songs
                .Where(x => x.BandId == bandId)
                .FirstOrDefault(x => x.Title == title);
            return s;
        }

        public IEnumerable<string> GetComposerList(int bandId)
        {
            var list = _context.Songs
                .Where(x => x.BandId == bandId)
                .Where(x => x.Composer.Length > 0)
                .Select(x => x.Composer)
                .Distinct()
                .ToList();
            return list;
        }

        public IEnumerable<SongMemberInstrument> GetMemberInstrumentList(int bandId)
        {
            var sim = _context.SongMemberInstruments
                .Where(x => x.BandId == bandId)
                .ToList();

            return sim;
        }

        public int Add(Song s)
        {
            _context.Songs.Add(s);
            _context.SaveChanges();
            return _context.Songs.Max(x => x.Id);
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(Song s)
        {
            _context.Songs.Remove(s);
            _context.SaveChanges();
        }
    }
}
