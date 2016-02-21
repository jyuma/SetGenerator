using Newtonsoft.Json;
using SetGenerator.Service;
using System.Collections.Generic;
using System.Linq;

namespace SetGenerator.WebUI.Common
{
    public class TableColumnDetail
    {
        public int Id { get; set; }
        public string Header { get; set; }
        public string Data { get; set; }
        public bool IsVisible { get; set; }
        public bool AlwaysVisible { get; set; }
        public bool IsMemberColumn { get; set; }
        public int TableColumnId { get; set; }
    }

    public class CommonSong
    {
        private readonly IAccount _account;
        private readonly string _currentUserName;

        public CommonSong( IAccount account, string currentUserName)
        {
            _account = account;
            _currentUserName = currentUserName;
        }

        public void SaveColumns(string columns, int tableId)
        {
            // gather/save table columns
            var cList = JsonConvert.DeserializeObject<IList<TableColumnDetail>>(columns);
            var cols = cList.Where(x => !x.IsMemberColumn).ToDictionary(c => c.Id, c => c.IsVisible);
            _account.UpdateUserPreferenceTableColumns(_currentUserName, cols);

            if (!cList.Any(x => x.IsMemberColumn)) return;

            cols.Clear();

            // gather/save member columns
            cols = cList.Where(x => x.IsMemberColumn).ToDictionary(c => c.Id, c => c.IsVisible);
            _account.UpdateUserPreferenceTableMembers(_currentUserName, cols);
        }

        public ICollection<TableColumnDetail> GetTableColumnList(int userId, int userTableId, int? bandId = null)
        {
            var tableColumns = _account.GetTableColumnsByBandId(userId, userTableId, bandId);
            
            var list = new List<TableColumnDetail>();

            list.AddRange(tableColumns
                .Where(x => x.TableColumn.Table.Id == userTableId)
                .OrderBy(x => x.TableColumn.Sequence)
                .Select(tc => new TableColumnDetail
            {
                Id = tc.Id,
                Header = tc.TableColumn.Name,
                Data = tc.TableColumn.Data,
                IsVisible = tc.IsVisible,
                AlwaysVisible = tc.TableColumn.AlwaysVisible,
                IsMemberColumn = false,
                TableColumnId = tc.TableColumn.Id
            }));

            if (bandId == null) return list;
            var tableMembers = _account.GetTableMembersByBandId(userId, userTableId, (int)bandId);
            list.AddRange(tableMembers
                .Where(x => x.Table.Id == userTableId)
                .OrderBy(o => o.Member.Alias)
                .Select(tc => new TableColumnDetail
                {
                    Id = tc.Id,
                    Header = tc.Member.Alias,
                    Data = tc.Member.Alias.ToLower(),
                    IsVisible = tc.IsVisible,
                    AlwaysVisible = false,
                    IsMemberColumn = true,
                    TableColumnId = 0
                }));
            return list;
        }
    }
}