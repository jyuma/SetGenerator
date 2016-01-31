using Newtonsoft.Json;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SetGenerator.Data.Repositories;
using SetGenerator.Domain.Entities;

namespace SetGenerator.WebUI.Common
{
    public class CommonSong
    {
        private readonly IKeyRepository _keyRepository;
        private readonly IAccount _account;
        private readonly IMemberRepository _memberRepository;
        private readonly string _currentUserName;

        public CommonSong( IAccount account, 
                                    IKeyRepository keyRepository, 
                                    IMemberRepository memberRepository,
                                    string currentUserName)
        {
            _keyRepository = keyRepository;
            _account = account;
            _memberRepository = memberRepository;
            _currentUserName = currentUserName;
        }

        public void SaveColumns(string columns, int tableId)
        {
            var cList = JsonConvert.DeserializeObject<IList<TableColumnDetail>>(columns);
            var cols = new OrderedDictionary();
            foreach (var c in cList)
                cols.Add(c.Data, c.IsVisible);
            _account.UpdateUserTablePreferences(_currentUserName, tableId, cols);
        }

        public IList<BandMemberDetail> GetBandMemberDetails(int bandId)
        {
            var list = new List<BandMemberDetail>();
            var members = _memberRepository.GetAll().Where( x => x.Band.Id == bandId);

            foreach (var m in members)
            {
                var bandmember = new BandMemberDetail { Id = m.Id, FirstName = m.FirstName, LastName = m.LastName, Initials = m.Initials, MemberInstrumentDetails = new List<BandMemberInstrumentDetail>() };
                foreach (var smid in m.MemberInstruments.Select(i => new BandMemberInstrumentDetail { Id = i.Instrument.Id, Name = i.Instrument.Name, Abbreviation = i.Instrument.Abbreviation }))
                {
                    bandmember.MemberInstrumentDetails.Add(smid);
                }
                list.Add(bandmember);
            }
            return list;
        }

        public IEnumerable<SongKeyDetail> GetKeyListFull()
        {
            var keys = _keyRepository.GetAll();

            return keys.Select(key => new SongKeyDetail
            {
                Id = key.Id,
                NameId = key.KeyName.Id,
                Name = key.KeyName.Name,
                SharpFlatNatural = key.SharpFlatNatural,
                MajorMinor = key.MajorMinor
            }).ToArray();
        }

        public ICollection<TableColumnDetail> GetTableColumnList(IEnumerable<UserPreferenceTableColumn> columns, IEnumerable<UserPreferenceTableMember> members, int userTableId)
        {
            var list = new List<TableColumnDetail>();

            list.AddRange(columns
                .Where(x => x.TableColumn.Table.Id == userTableId)
                .Select(tc => new TableColumnDetail
            {
                Header = tc.TableColumn.Name,
                Data = tc.TableColumn.Data,
                IsVisible = tc.IsVisible,
                AlwaysVisible = tc.TableColumn.AlwaysVisible,
                IsMember = false
            }));

            list.AddRange(members
                .Where(x => x.Table.Id == userTableId)
                .Select(tc => new TableColumnDetail
            {
                Header = tc.Member.FirstName,
                Data = tc.Member.FirstName.ToLower(),
                IsVisible = tc.IsVisible,
                AlwaysVisible = false,
                IsMember = true
            }));

            return list;
        }
    }
}