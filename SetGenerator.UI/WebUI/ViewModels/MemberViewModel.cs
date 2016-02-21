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
        public int DefaultInstrumentId { get; set; }
        public int[] InstrumentIds { get; set; }
        public string UserUpdate { get; set; }
        public string DateUpdate { get; set; }
    }

    public class MemberViewModel
    {
        public string BandName { get; set; }
        public int BandId { get; set; }
        public int SelectedId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }

    public class MemberEditViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Alias { get; set; }
        public SelectList MemberInstruments { get; set; }
    }

    public class InstrumentDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}