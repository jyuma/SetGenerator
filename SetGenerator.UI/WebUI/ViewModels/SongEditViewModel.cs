using System.Collections.Generic;
using System.Web.Mvc;

namespace SetGenerator.WebUI.ViewModels
{
    public class MemberInstrumentDetail
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public SelectList Instruments { get; set; }
    }

    public class SongEditViewModel
    {
        public string Title { get; set; }
        public string Composer { get; set; }
        public SelectList KeyNames { get; set; }
        public SelectList SharpFlatNatural{ get; set; }
        public SelectList MajorMinor { get; set; }
        public SelectList Members { get; set; }
        public SelectList Genres { get; set; }
        public SelectList Tempos { get; set; }
        public bool NeverOpen { get; set; }
        public bool NeverClose { get; set; }
        public IEnumerable<MemberInstrumentDetail> MemberInstruments { get; set; }
        public IEnumerable<string> Composers { get; set; }
    }
}