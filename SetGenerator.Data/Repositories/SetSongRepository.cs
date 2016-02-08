using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface ISetSongRepository : IRepositoryBase<SetSong>
    {
        SetSong GetBySetlistSong(int setlistId, int songId);
    }

    public class SetSongRepository : RepositoryBase<SetSong>, ISetSongRepository
    {
        public SetSongRepository(ISession session)
            : base(session)
        {
        }

        public SetSong GetBySetlistSong(int setlistId, int songId)
        {
            return Session.QueryOver<SetSong>()
                .Where(x => x.Setlist.Id == setlistId)
                .Where(x => x.Song.Id == songId)
                .SingleOrDefault();
        }
    }
}
