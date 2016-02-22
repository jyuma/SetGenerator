using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        User GetByUserName(string uname);
        IList<UserPreferenceTableColumn> GetTableColumnsByBandId(int userId, int tableId, int? bandId);
        IList<UserPreferenceTableMember> GetTableMembersByBandId(int userId, int tableId, int bandId);
        IList<UserBand> GetUserBands(string uname);
        ArrayList GetDefaultBandArrayList(int userId);

        // UserBand
        int AddUserBand(int userId, int bandId);
        void DeleteUserBand(int userId, int bandId);

        // UserPreferenceTableColumns
        int AddUserPreferenceTableColumns(int userId, int bandId);
        void DeleteUserPreferenceTableColumns(int userId, int bandId);

        // UserPreferenceTableMember
        int AddUserPreferenceTableMember(int userId, int memberId);
        void DeleteUserPreferenceTableMember(int userId, int memberId);
        void DeleteUserPreferenceTableMembers(int userId, int bandId);
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

        public IList<UserPreferenceTableColumn> GetTableColumnsByBandId(int userId, int tableId, int? bandId)
        {
            IList<UserPreferenceTableColumn> list = null;

            if (bandId != null)
            {
                list = Session.QueryOver<UserPreferenceTableColumn>()
                    .Where(x => x.User.Id == userId)
                    .Where(x => x.Band.Id == bandId)
                    .JoinQueryOver(uptc => uptc.TableColumn)
                    .Where(x => x.Table.Id == tableId)
                    .List()
                    .OrderBy(o => o.TableColumn.Sequence)
                    .ToList();
            }
            else
            {
                list = Session.QueryOver<UserPreferenceTableColumn>()
                    .Where(x => x.User.Id == userId)
                    .JoinQueryOver(uptc => uptc.TableColumn)
                    .Where(x => x.Table.Id == tableId)
                    .List()
                    .OrderBy(o => o.TableColumn.Sequence)
                    .ToList();
            }

            return list;
        }

        public IList<UserPreferenceTableMember> GetTableMembersByBandId(int userId, int tableId, int bandId)
        {
            var list = Session.QueryOver<UserPreferenceTableMember>()
                .Where(x => x.User.Id == userId)
                .Where(x => x.Table.Id == tableId)
                .JoinQueryOver(uptm => uptm.Member)
                .Where(x => x.Band.Id == bandId)
                .List()
                .OrderBy(o => o.Member.FirstName)
                .ThenBy(o => o.Member.LastName)
                .ToList();

            return list;
        }

        public IList<UserBand> GetUserBands(string uname)
        {
            if (uname.Length == 0) return null;

            var user = GetByUserName(uname);

            var list = Session.QueryOver<User>()
                .Where(x => x.Id == user.Id)
                .SingleOrDefault()
                .UserBands;

            return list;
        }

        public ArrayList GetDefaultBandArrayList(int userId)
        {
            var bands = GetDefaultUserBands(userId);
            var al = new ArrayList();

            foreach (var b in bands)
                al.Add(new { Value = b.Key, Display = b.Value });

            return al;
        }

        private Dictionary<int, string> GetDefaultUserBands(int userId)
        {
            Member bandTableAlias = null;

            return Session.QueryOver<User>()
                .Where(x => x.DefaultBand != null)
                .JoinAlias(x => x.DefaultBand, () => bandTableAlias)
                .Where(x => x.DefaultBand.Id == bandTableAlias.Id)
                .List()
                .Select(x => new
                {
                    x.DefaultBand.Id,
                    x.DefaultBand.Name
                })
                .Distinct()
                .OrderBy(o => o.Name)
                .ToDictionary(x => x.Id, y => y.Name);
        }

        // UserBand
        public int AddUserBand(int userId, int bandId)
        {
            int id;
            var user = Session.QueryOver<User>().Where(x => x.Id == userId).SingleOrDefault();
            var band = Session.QueryOver<Band>().Where(x => x.Id == bandId).SingleOrDefault();

            var userBand = new UserBand
            {
                User = user, 
                Band = band
            };

            using (var transaction = Session.BeginTransaction())
            {
                id = (int)Session.Save(userBand);
                transaction.Commit();
            }

            return id;
        }

        public void DeleteUserBand(int userId, int bandId)
        {
            var user = Session.QueryOver<User>()
                .Where(x => x.Id == userId).SingleOrDefault();

            var userBand = user.UserBands
                .SingleOrDefault(x => x.Band.Id == bandId);

            if (userBand == null) return;

            user.UserBands.Remove(userBand);
            using (var transaction = Session.BeginTransaction())
            {
                Session.Delete(userBand);
                transaction.Commit();
            }
        }

        // UserPreferenceTableColumns

        public int AddUserPreferenceTableColumns(int userId, int bandId)
        {
            const int bandTableId = 1;
            var id = 0;
            var user = Session.QueryOver<User>().Where(x => x.Id == userId).SingleOrDefault();
            var band = Session.QueryOver<Band>().Where(x => x.Id == bandId).SingleOrDefault();
            var tableColumns = Session.QueryOver<TableColumn>().Where(x => x.Table.Id != bandTableId).List();

            var userPreferenceTableColumns = tableColumns.Select(x => new
                UserPreferenceTableColumn
                {
                    User = user,
                    Band = band,
                    TableColumn = x,
                    IsVisible = x.AlwaysVisible
                });

            foreach (var column in userPreferenceTableColumns)
            {
                using (var transaction = Session.BeginTransaction())
                {
                    id = (int)Session.Save(column);
                    transaction.Commit();
                }
            }

            return id;
        }

        public void DeleteUserPreferenceTableColumns(int userId, int bandId)
        {
            var user = Session.QueryOver<User>().Where(x => x.Id == userId).SingleOrDefault();

            var userPreferenceTableColumns = user.UserPreferenceTableColumns
                .Where(x => x.Band != null)
                .Where(x => x.Band.Id == bandId).ToArray();

            if (!userPreferenceTableColumns.Any()) return;

            foreach (var column in userPreferenceTableColumns)
            {
                user.UserPreferenceTableColumns.Remove(column);
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(column);
                    transaction.Commit();
                }
            }
        }

        // UserPreferenceTableMembers

        public int AddUserPreferenceTableMember(int userId, int memberId)
        {
            int id;
            var user = Session.QueryOver<User>().Where(x => x.Id == userId).SingleOrDefault();
            var member = Session.QueryOver<Member>().Where(x => x.Id == memberId).SingleOrDefault();

            const int tableSongId = 2;
            const int tableSetId = 5;

            var tableSong = Session.QueryOver<Table>().Where(x => x.Id == tableSongId).SingleOrDefault();
            var tableSet = Session.QueryOver<Table>().Where(x => x.Id == tableSetId).SingleOrDefault();

            var userPreferenceTableMember = new UserPreferenceTableMember
            {
                User = user,
                Member = member,
                Table = tableSong,
                IsVisible = false
            };

            using (var transaction = Session.BeginTransaction())
            {
                id = (int)Session.Save(userPreferenceTableMember);
                transaction.Commit();
            }

            userPreferenceTableMember = new UserPreferenceTableMember
            {
                User = user,
                Member = member,
                Table = tableSet,
                IsVisible = false
            };

            using (var transaction = Session.BeginTransaction())
            {
                id = (int)Session.Save(userPreferenceTableMember);
                transaction.Commit();
            }

            return id;
        }

        public void DeleteUserPreferenceTableMember(int userId, int memberId)
        {
            var user = Session.QueryOver<User>().Where(x => x.Id == userId).SingleOrDefault();

            var userPreferenceTableMember = user.UserPreferenceTableMembers
                .SingleOrDefault(x => x.Member.Id == memberId);

            if (userPreferenceTableMember == null) return;

            user.UserPreferenceTableMembers.Remove(userPreferenceTableMember);
            using (var transaction = Session.BeginTransaction())
            {
                Session.Delete(userPreferenceTableMember);
                transaction.Commit();
            }
        }

        public void DeleteUserPreferenceTableMembers(int userId, int bandId)
        {
            var user = Session.QueryOver<User>().Where(x => x.Id == userId).SingleOrDefault();

            IList<int> memberIds = Session.QueryOver<Band>().Where(x => x.Id == bandId)
                .SingleOrDefault()
                .Members.Select(x => x.Id)
                .ToArray();

            const int tableSongId = 2;
            DeleteUserPreferenceTableMembers(user, tableSongId, memberIds);
            
            const int tableSetId = 5;
            DeleteUserPreferenceTableMembers(user, tableSetId, memberIds);
        }

        private void DeleteUserPreferenceTableMembers(User user, int tableId, IEnumerable<int> memberIds)
        {
            var userPreferenceTableMembers = user.UserPreferenceTableMembers
                .Where(x => x.Table.Id == tableId)
                .Where(x => memberIds.Contains(x.Member.Id)).ToArray();

            if (!userPreferenceTableMembers.Any()) return;

            foreach (var userPreferenceTableMember in userPreferenceTableMembers)
            {
                user.UserPreferenceTableMembers.Remove(userPreferenceTableMember);
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(userPreferenceTableMember);
                    transaction.Commit();
                }
            }
        }
    }
}
