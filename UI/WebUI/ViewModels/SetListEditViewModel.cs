using System.Web.Mvc;

namespace SetGenerator.WebUI.ViewModels
{
    public class SetListEditViewModel
    {
        public int SetListId { get; set; }
        public string Name { get; set; }
        public SelectList ToalSetsList { get; set; }
        public SelectList TotalSongsPerSetList { get; set; }
    }
}