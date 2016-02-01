using System.Collections;
using System.Collections.Generic;

namespace SetGenerator.WebUI.ViewModels
{
    public class SetlistDetail
    {
        public int Id;
        public string Name;
        public int NumSets { get; set; }
        public int NumSongs { get; set; }
        public string UserUpdate;
        public string DateUpdate;
    }

    public class SetSongDetail : SongDetail
    {
        public int SetNumber;
    }

    public class SetlistViewModel
    {
        public IEnumerable<SetlistDetail> SetlistList { get; set; }
        public ArrayList MemberArrayList { get; set; }
        public ArrayList KeyNameArrayList { get; set; }
        public ArrayList InstrumentArrayList { get; set; }
        public IEnumerable<string> KeyNameList { get; set; }
        public ArrayList UserBandArrayList { get; set; }
        public IEnumerable<TableColumnDetail> TableColumnList { get; set; }

        public int SelectedId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }

    public class SetViewModel
    {
        public string Name { get; set; }
        public ICollection<int> SetNumberList { get; set; }
        public int SetlistId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }
}