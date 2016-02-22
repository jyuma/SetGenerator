using System.Collections.Generic;
using System.Web.Mvc;

namespace SetGenerator.WebUI.ViewModels
{
    public class BandDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DefaultSingerId { get; set; }
        public string UserUpdate { get; set; }
        public string DateUpdate { get; set; }
    }

    public class BandViewModel
    {
        public IEnumerable<BandDetail> BandList { get; set; }

        public int SelectedId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }


    public class BandEditViewModel
    {
        public string Name { get; set; }
        public SelectList Members { get; set; }
    }
}