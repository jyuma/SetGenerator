using System.Collections.Generic;
using System.Web.Mvc;

namespace SetGenerator.WebUI.ViewModels
{
    public class SongDetail
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int KeyId { get; set; }
        public SongKeyDetail KeyDetail { get; set; }
        public int SingerId { get; set; }
        public bool NeverOpen { get; set; }
        public bool NeverClose { get; set; }
        public bool Disabled { get; set; }
        public string Composer { get; set; }
        public int GenreId { get; set; }
        public int TempoId { get; set; }
        public string UserUpdate { get; set; }
        public string DateUpdate { get; set; }
        public IEnumerable<SongMemberInstrumentDetail> SongMemberInstrumentDetails { get; set; }
    }

    public class SongMemberInstrumentDetail
    {
        public int MemberId { get; set; }
        public int InstrumentId { get; set; }
        public string InstrumentAbbrev { get; set; }
        public string InstrumentName { get; set; }
    }

    public class SongKeyDetail
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int NameId { get; set; }
        public int SharpFlatNatural { get; set; }
        public int MajorMinor { get; set; }
    }

    public class SongViewModel
    {
        public int SelectedId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
        public string BandName { get; set; }
    }

    public class MemberInstrumentDetail
    {
        public int BandId { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public SelectList Instruments { get; set; }
        public int[] InstrumentIds { get; set; }
    }

    public class SongEditViewModel
    {
        public string Title { get; set; }
        public string Composer { get; set; }
        public SelectList KeyNames { get; set; }
        public SelectList SharpFlatNatural { get; set; }
        public SelectList MajorMinor { get; set; }
        public SelectList Singers { get; set; }
        public SelectList Genres { get; set; }
        public SelectList Tempos { get; set; }
        public bool NeverOpen { get; set; }
        public bool NeverClose { get; set; }
        public IEnumerable<MemberInstrumentDetail> MemberInstruments { get; set; }
        public IEnumerable<string> Composers { get; set; }
    }
}
