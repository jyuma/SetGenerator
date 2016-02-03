using System;
using System.Collections.Generic;

namespace SetGenerator.WebUI.ViewModels
{
    public class GigEditViewModel
    {
        public DateTime DateGig { get; set; }
        public string Venue { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Venues { get; set; }
    }
}