using Newtonsoft.Json;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SetGenerator.Domain.Entities;

namespace SetGenerator.WebUI.Common
{
    public class CommonSong
    {
        private readonly IAccount _account;
        private readonly string _currentUserName;

        public CommonSong( IAccount account, 
                           string currentUserName)
        {
            _account = account;
            _currentUserName = currentUserName;
        }

        public void SaveColumns(string columns, int tableId)
        {
            var cList = JsonConvert.DeserializeObject<IList<TableColumnDetail>>(columns);
            var cols = new OrderedDictionary();

            foreach (var c in cList.Where(x => x.MemberId == 0))
                cols.Add(c.Data, c.IsVisible);
            _account.UpdateUserPreferenceTableColumns(_currentUserName, tableId, cols);

            cols.Clear();
            foreach (var c in cList.Where(x => x.MemberId > 0))
                cols.Add(c.MemberId.ToString(), c.IsVisible);
            _account.UpdateUserPreferenceTableMembers(_currentUserName, tableId, cols);
        }

        public ICollection<TableColumnDetail> GetTableColumnList(IEnumerable<UserPreferenceTableColumn> columns, IEnumerable<UserPreferenceTableMember> members, int userTableId)
        {
            var list = new List<TableColumnDetail>();

            list.AddRange(columns
                .Where(x => x.TableColumn.Table.Id == userTableId)
                .OrderBy(x => x.TableColumn.Sequence)
                .Select(tc => new TableColumnDetail
            {
                Header = tc.TableColumn.Name,
                Data = tc.TableColumn.Data,
                IsVisible = tc.IsVisible,
                AlwaysVisible = tc.TableColumn.AlwaysVisible,
                MemberId = 0
            }));

            list.AddRange(members
                .Where(x => x.Table.Id == userTableId)
                .OrderBy(o => o.Member.FirstName)
                .ThenBy(o => o.Member.LastName)
                .Select(tc => new TableColumnDetail
            {
                Header = tc.Member.FirstName,
                Data = tc.Member.FirstName.ToLower(),
                IsVisible = tc.IsVisible,
                AlwaysVisible = false,
                MemberId = tc.Member.Id
            }));

            return list;
        }
    }
}