using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface ITableColumnRepository : IRepositoryBase<TableColumn>
    {
        Table GetTable(int tableId);
    }

    public class TableColumnRepository : RepositoryBase<TableColumn>, ITableColumnRepository
    {
        public TableColumnRepository(ISession session)
            : base(session)
        {
        }

        public Table GetTable(int tableId)
        {
            return Session.QueryOver<Table>()
                .Where(x => x.Id == tableId)
                .SingleOrDefault();
        }
    }
}
