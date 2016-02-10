using System.Collections.Generic;

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

    public class BandMemberDetail
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Initials { get; set; }
        public IEnumerable<BandMemberInstrumentDetail> MemberInstrumentDetails { get; set; }
    }

    public class BandMemberInstrumentDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
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
        public string BandName { get; set; }
        public int SelectedId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }

    public class SongGenre
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SongTempo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class EntityAudit
    { }
}
