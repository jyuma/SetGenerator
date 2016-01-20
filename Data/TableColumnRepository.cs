using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain;

namespace SetGenerator.Repositories
{
    public interface ITableColumnRepository
    {
        TableColumn GetSingle(int id);
        IEnumerable<TableColumn> GetList();
    }

    public class TableColumnRepository : ITableColumnRepository
    {
        private readonly SetGeneratorEntities _context;

        public TableColumnRepository(SetGeneratorEntities context)
        {
            _context = context;
        }

        public TableColumn GetSingle(int id)
        {
            var tc = _context.TableColumns
                .FirstOrDefault(x => x.Id == id);
            return tc;
        }

        public IEnumerable<TableColumn> GetList()
        {
            var list = _context.TableColumns
                 .OrderBy(x => x.Id)
                 .ToList();
            return list;
        }
    }
}
