using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain;

namespace SetGenerator.Repositories
{
    public interface IUserRepository
    {
        User GetSingle(long id);
        User GetByUserName(string uname);
        IEnumerable<User> GetList();
        void Add(User up);
        void Delete(User up);
        void Update();

        // admin stuff
        //bool IsAdminAccessDenied(string ipAddress);
        //void AddAdminAccessDenied(string ipAddress);
    }

    public class UserRepository : IUserRepository
    {
        private readonly SetGeneratorEntities _context;

        public UserRepository(SetGeneratorEntities context)
        {
            _context = context;
            _context.Configuration.LazyLoadingEnabled = false;
        }

        public IQueryable<User> ListOfUsers
        {
            get
            {
                return _context.Users                    
                    .Include("UserPreferenceTableMembers.Member")
                    .Include("UserPreferenceTableColumns.TableColumn.Table")
                    .Include("UserBands.Band");
            }
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(User u)
        {
            _context.Users.Remove(u);
            _context.SaveChanges();
        }

        public void Add(User u)
        {
            _context.Users.Add(u);
            _context.SaveChanges();
        }

        public User GetByUserName(string uname)
        {
            var u = _context.Users
                    .Include("UserPreferenceTableMembers.Member")
                    .Include("UserPreferenceTableColumns.TableColumn.Table")
                    .Include("UserBands.Band")
                    .FirstOrDefault(x => x.UserName == uname);
            return u;
        }

        public User GetSingle(long id)
        {
            User u = _context.Users
                             .FirstOrDefault(x => x.Id == id);
            return u;
        }

        public IEnumerable<User> GetList()
        {
            var list = _context.Users
                    .Include("UserPreferenceTableMembers.Member")
                    .Include("UserPreferenceTableColumns.TableColumn.Table")
                    .Include("UserBands.Band")
                    .ToList();
            return list;
        }


        // admin stuff

        //public bool IsAdminAccessDenied(string ipAddress)
        //{
        //    return (_context.DeniedAccesses.Count(x => x.IPAddress == ipAddress) > 0);
        //}

        //public void AddAdminAccessDenied(string ipAddress)
        //{
        //    _context.DeniedAccesses.Add(new DeniedAccess { IPAddress = ipAddress, DateDenied = DateTime.Now });
        //    _context.SaveChanges();
        //}
    }
}
