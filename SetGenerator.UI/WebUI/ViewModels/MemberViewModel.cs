using System.Collections.Generic;
using System.Web.Mvc;

namespace SetGenerator.WebUI.ViewModels
{
    public class MemberDetail
    {
        public int Id { get; set; }
        public int BandId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Alias { get; set; }
        public bool IsSongMemberInstrument { get; set; }
        public int DefaultInstrumentId { get; set; }
    }

    public class MemberViewModel
    {
        public int SelectedId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
        public string BandName { get; set; }
    }

    public class MemberEditViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Alias { get; set; }
        public SelectList MemberInstruments { get; set; }
    }

    public class MemberInstrumentEditViewModel
    {
        public SelectList AssignedInstruments { get; set; }
        public SelectList AvailableInstruments { get; set; }
    }
}