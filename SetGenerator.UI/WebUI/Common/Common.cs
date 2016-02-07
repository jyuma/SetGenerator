﻿using Newtonsoft.Json;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using System.Collections.Generic;
using System.Linq;

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
            var cols = cList.Where(x => x.MemberId == 0).ToDictionary(c => c.Id, c => c.IsVisible);
            _account.UpdateUserPreferenceTableColumns(_currentUserName, cols);

            if (!cList.Any(x => x.MemberId > 0)) return;

            cols.Clear();
            cols = cList.Where(x => x.MemberId > 0).ToDictionary(c => c.Id, c => c.IsVisible);
            _account.UpdateUserPreferenceTableMembers(_currentUserName, cols);
        }

        public ICollection<TableColumnDetail> GetTableColumnList(int userId, int userTableId, int bandId)
        {
            var tableColumns = _account.GetTableColumnsByBandId(userId, userTableId, bandId);
            var tableMembers = _account.GetTableMembersByBandId(userId, userTableId, bandId);
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
                MemberId = 0            // indicate that it's not a member column
            }));

            list.AddRange(tableMembers
                .Where(x => x.Table.Id == userTableId)
                .OrderBy(o => o.Member.FirstName)
                .ThenBy(o => o.Member.LastName)
                .Select(tc => new TableColumnDetail
            {
                Id = tc.Id,
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