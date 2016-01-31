using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IKeyRepository : IRepositoryBase<Key>
    {
        IEnumerable<string> GetNameList();
        ArrayList GetNameArrayList();
    }

    public class KeyRepository : RepositoryBase<Key>, IKeyRepository
    {
        public KeyRepository(ISession session)
            : base(session)
        {
        }

        public IEnumerable<string> GetNameList()
        {
            return GetAll()
                .OrderBy(x => x.Id)
                .Select(item => item.KeyName.Name)
                .ToArray();
        }

        private IEnumerable<KeyName> GetArrayList()
        {
            return GetAll()
                .OrderBy(x => x.KeyName.Id)
                .Select(x => x.KeyName)
                .Distinct()
                .ToArray();
        }

        public ArrayList GetNameArrayList()
        {
            var keyNames = GetArrayList();
            if (keyNames == null) return null;
            var al = new ArrayList();

            foreach (var k in keyNames)
                al.Add(new { Value = k.Id, Display = k.Name });

            return al;
        }
    }
}