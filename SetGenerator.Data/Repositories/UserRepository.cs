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
        IList<UserBand> GetUserBands(int  userId);
        ArrayList GetDefaultBandArrayList();
        int AddRemoveUserBands(int userId, int[] bandIds);
        void DeleteUser(int userId);
        void AssignStartupUserPreferenceTableColumns(int userId);

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

        public IList<UserBand> GetUserBands(int userId)
        {
            var list = Session.QueryOver<User>()
                .Where(x => x.Id == userId)
                .SingleOrDefault().UserBands
                .OrderBy(o => o.Band.Name)
                .ToArray();

            return list;
        }

        public ArrayList GetDefaultBandArrayList()
        {
            var bands = GetDefaultUserBands();
            var al = new ArrayList();

            foreach (var b in bands)
                al.Add(new { Value = b.Key, Display = b.Value });

            return al;
        }

        public void DeleteUser(int userId)
        {
            DeleteAllUserPreferences(userId);

            var userAmin = Session.QueryOver<User>()
                .Where(x => x.UserName == "admin").SingleOrDefault();

            // update entity audit bands
            var bands = Session.QueryOver<Band>()
                .Where(x => x.UserUpdate.Id == userId || x.UserCreate.Id == userId)
                .List();

            foreach (var band in bands)
            {
                if (band.UserUpdate.Id == userId)
                    band.UserUpdate = userAmin;

                if (band.UserCreate.Id == userId)
                    band.UserCreate = userAmin;

                using (var transaction = Session.BeginTransaction())
                {
                    Session.Update(band);
                    transaction.Commit();
                }
            }

            // update entity audit gigs
            var gigs = Session.QueryOver<Gig>()
                .Where(x => x.UserUpdate.Id == userId || x.UserCreate.Id == userId)
                .List();

            foreach (var gig in gigs)
            {
                if (gig.UserUpdate.Id == userId)
                    gig.UserUpdate = userAmin;

                if (gig.UserCreate.Id == userId)
                    gig.UserCreate = userAmin;

                using (var transaction = Session.BeginTransaction())
                {
                    Session.Update(gig);
                    transaction.Commit();
                }
            }

            // update entity audit songs
            var songs = Session.QueryOver<Song>()
                .Where(x => x.UserUpdate.Id == userId || x.UserCreate.Id == userId)
                .List();

            foreach (var song in songs)
            {
                if (song.UserUpdate.Id == userId)
                    song.UserUpdate = userAmin;

                if (song.UserCreate.Id == userId)
                    song.UserCreate = userAmin;

                using (var transaction = Session.BeginTransaction())
                {
                    Session.Update(song);
                    transaction.Commit();
                }
            }

            // update entity audit setlists
            var setlists = Session.QueryOver<Setlist>()
                .Where(x => x.UserUpdate.Id == userId || x.UserCreate.Id == userId)
                .List();

            foreach (var setlist in setlists)
            {
                if (setlist.UserUpdate.Id == userId)
                    setlist.UserUpdate = userAmin;

                if (setlist.UserCreate.Id == userId)
                    setlist.UserCreate = userAmin;

                using (var transaction = Session.BeginTransaction())
                {
                    Session.Update(setlist);
                    transaction.Commit();
                }
            }

            Delete(userId);
        }

        private void DeleteAllUserPreferences(int userId)
        {
            var userBands = GetUserBands(userId);

            foreach (var userBand in userBands)
            {
                DeleteUserPreferenceTableColumns(userId, userBand.Band.Id);
                DeleteUserPreferenceTableColumns(userId, userBand.Band.Id);
            }
        }

        private Dictionary<int, string> GetDefaultUserBands()
        {
            var userBands = Session.QueryOver<User>()
                .List()
                .Select(x => x.DefaultBand)
                .Where(x => x != null)
                .OrderBy(o => o.Name)
                .Distinct();

            return userBands
                .Select(x => new
                {
                    x.Id,
                    x.Name
                })
                .OrderBy(o => o.Name)
                .ToDictionary(x => x.Id, y => y.Name);
        }

        public int AddRemoveUserBands(int userId, int[] bandIds)
        {
            int id = 0;
            var user = Session.QueryOver<User>().Where(x => x.Id == userId).SingleOrDefault();
            var existingIds = user.UserBands.Select(x => x.Band.Id).ToArray();

            var addIds = bandIds
                .Select(x => x)
                .Where(x => !existingIds.Contains(x))
                .ToArray();

            if (addIds.Any())
                id = AddUserBands(user, addIds);

            var removeIds = existingIds
                .Select(x => x)
                .Where(x => !bandIds.Contains(x))
                .ToArray();

            if (!removeIds.Any()) return id;

            RemoveUserBands(user, removeIds);

            if (user.DefaultBand != null)
            {
                if (removeIds.Contains(user.DefaultBand.Id))
                {
                    user.DefaultBand = null;
                    Session.Update(user);
                }
            }

            return id;
        }

        private int AddUserBands(User user, IEnumerable<int> bandIds)
        {
            var result = 0;

            foreach (var id in bandIds)
            {
                var idLocal = id;
                var band = Session.QueryOver<Band>()
                    .Where(x => x.Id == idLocal)
                    .SingleOrDefault();

                var userBand = new UserBand
                {
                    User = user,
                    Band = band
                };
                
                using (var transaction = Session.BeginTransaction())
                {
                    result = (int) Session.Save(userBand);
                    transaction.Commit();
                }

                AddUserPreferenceTableColumns(user.Id, idLocal);
                AddUserPreferenceTableMembers(user.Id, idLocal);
            }

            return result;
        }

        private void RemoveUserBands(User user, IEnumerable<int> bandIds)
        {
            foreach (var userBand in bandIds
                .Select(removeIdLocal => user.UserBands
                .SingleOrDefault(x => x.Band.Id == removeIdLocal)))
            {
                user.UserBands.Remove(userBand);
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(userBand);
                    transaction.Commit();
                }
                DeleteUserPreferenceTableColumns(user.Id, userBand.Band.Id);
                DeleteUserPreferenceTableMembers(user.Id, userBand.Band.Id);
            }
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
            const int userTableId = 7;
            var id = 0;

            var user = Session.QueryOver<User>().Where(x => x.Id == userId).SingleOrDefault();
            var band = Session.QueryOver<Band>().Where(x => x.Id == bandId).SingleOrDefault();
            var tableColumns = Session.QueryOver<TableColumn>()
                .Where(x => x.Table.Id != bandTableId)
                .Where(x => x.Table.Id != userTableId)
                .List();

            var userPreferenceTableColumns = tableColumns.Select(x => new
                UserPreferenceTableColumn
                {
                    User = user,
                    Band = band,
                    TableColumn = x,
                    IsVisible = (x.Data != "updateuser" && x.Data != "updatedate")
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

        public void AssignStartupUserPreferenceTableColumns(int userId)
        {
            const int userTableBand = 1;
            var user = Session.QueryOver<User>().Where(x => x.Id == userId).SingleOrDefault();

            var columns = Session.QueryOver<TableColumn>()
                .List()
                .Where(x => x.Table.Id == userTableBand);

            var userPrefTableColumns = columns.Select(x => new UserPreferenceTableColumn
            {
                User = user,
                TableColumn = x,
                IsVisible = (x.Data != "updateuser" && x.Data != "updatedate")
            }).ToList();

            foreach (var tableColumn in userPrefTableColumns)
            {
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Save(tableColumn);
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

        private int AddUserPreferenceTableMembers(int userId, int bandId)
        {
            var result = -1;
            
            IList<int> memberIds = Session.QueryOver<Band>().Where(x => x.Id == bandId)
                .SingleOrDefault()
                .Members.Select(x => x.Id)
                .ToArray();

            foreach (var memberId in memberIds)
            {
                result = AddUserPreferenceTableMember(userId, memberId);
            }

            return result;
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
