using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface ITableColumnRepository : IRepositoryBase<TableColumn>
    {
    }

    public class TableColumnRepository : RepositoryBase<TableColumn>, ITableColumnRepository
    {
        public TableColumnRepository(ISession session)
            : base(session)
        {
        }
    }
}
