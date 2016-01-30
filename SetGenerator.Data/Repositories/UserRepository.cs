using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        User GetByUserName(string uname);
    }

    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(ISession session)
            : base(session)
        {
        }

        public User GetByUserName(string uname)
        {
            var u = GetAll()
                    .FirstOrDefault(x => x.UserName == uname);
            return u;
        }

    }
}
