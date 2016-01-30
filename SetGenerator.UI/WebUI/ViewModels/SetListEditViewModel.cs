using System.Web.Mvc;

namespace SetGenerator.WebUI.ViewModels
{
    public class SetlistEditViewModel
    {
        public int SetlistId { get; set; }
        public string Name { get; set; }
        public SelectList ToalSetsList { get; set; }
        public SelectList TotalSongsPerSetlist { get; set; }
    }
}