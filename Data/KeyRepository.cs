using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain;

namespace SetGenerator.Repositories
{
    public interface IKeyRepository
    {
        Key GetSingle(int id);
        IEnumerable<Key> GetList();
        IEnumerable<string> GetNameList();
        ArrayList GetNameArrayList();
    }

    public class KeyRepository : IKeyRepository
    {
        private readonly SetGeneratorEntities _context;

        public KeyRepository(SetGeneratorEntities context)
        {
            _context = context;
        }

        public Key GetSingle(int id)
        {
            var k = _context.Keys
                .FirstOrDefault(x => x.Id == id);
            return k;
        }

        public IEnumerable<Key> GetList()
        {
            var list = _context.Keys
                    .OrderBy(x => x.Id)
                    .ToList();

            return list;
        }

        public IEnumerable<string> GetNameList()
        {
            return _context.KeyNames
                .OrderBy(x => x.Id)
                .Select(item => item.Name)
                .ToList();
        }

        private IEnumerable<KeyName> GetArrayList()
        {
            return _context.KeyNames
                .OrderBy(x => x.Id)
                .ToList();
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
