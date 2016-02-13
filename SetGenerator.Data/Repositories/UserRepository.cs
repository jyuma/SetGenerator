using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        User GetByUserName(string uname);
        IEnumerable<UserPreferenceTableColumn> GetTableColumnsByBandId(int userId, int tableId, int bandId);
        IEnumerable<UserPreferenceTableMember> GetTableMembersByBandId(int userId, int tableId, int bandId);
        IEnumerable<UserBand> GetUserBands(string uname);
    }

    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(ISession session)
            : base(session)
        {
        }

        public User GetByUserName(string uname)
        {
            var u = Session.QueryOver<User>()
                .Where(x => x.UserName == uname)
                .SingleOrDefault();
            return u;
        }

        public IEnumerable<UserPreferenceTableColumn> GetTableColumnsByBandId(int userId, int tableId, int bandId)
        {
            var list = Session.QueryOver<UserPreferenceTableColumn>()
                .Where(x => x.User.Id == userId)
                .Where(x => x.Band.Id == bandId)
                .JoinQueryOver(uptc => uptc.TableColumn)
                .Where(x => x.Table.Id == tableId)
                .List()
                .OrderBy(o => o.TableColumn.Sequence);

            return list;
        }

        public IEnumerable<UserPreferenceTableMember> GetTableMembersByBandId(int userId, int tableId, int bandId)
        {
            var list = Session.QueryOver<UserPreferenceTableMember>()
                .Where(x => x.User.Id == userId)
                .Where(x => x.Table.Id == tableId)
                .JoinQueryOver(uptm => uptm.Member)
                .Where(x => x.Band.Id == bandId)
                .List()
                .OrderBy(o => o.Member.FirstName)
                .ThenBy(o => o.Member.LastName);

            return list;
        }

        public IEnumerable<UserBand> GetUserBands(string uname)
        {
            if (uname.Length == 0) return null;

            var user = GetByUserName(uname);

            var list = Session.QueryOver<User>()
                .Where(x => x.Id == user.Id)
                .SingleOrDefault()
                .UserBands;

            return list;
        }
    }
}
