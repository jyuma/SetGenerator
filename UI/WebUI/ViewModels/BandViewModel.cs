using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SetGenerator.WebUI.ViewModels
{
    public class Bandetail
    {
        public int Id { get; set; }
        public string DateCreated { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UserUpdate { get; set; }
        public string DateUpdate { get; set; }
    }

    public class BandViewModel
    {
        public IList<Bandetail> BandList { get; set; }
        public IList<TableColumnDetail> TableColumnList { get; set; }

        public int SelectedId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }
}