using System;
using System.Collections.Generic;

namespace SetGenerator.WebUI.ViewModels
{
    public class GigDetail
    {
        public int Id { get; set; }
        public int BandId { get; set; }
        public string DateGig { get; set; }
        public string Venue { get; set; }
        public string Description { get; set; }
        public string UserUpdate { get; set; }
        public string DateUpdate { get; set; }
    }

    public class GigViewModel
    {
        public string BandName { get; set; }
        public int SelectedId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }

    public class GigEditViewModel
    {
        public DateTime DateGig { get; set; }
        public string Venue { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Venues { get; set; }
    }
}