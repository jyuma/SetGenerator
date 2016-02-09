using System.Web.Mvc;

namespace SetGenerator.WebUI.ViewModels
{
    public class SetlistEditViewModel
    {
        public int SetlistId { get; set; }
        public string Name { get; set; }
        public SelectList TotalSetsList { get; set; }
        public SelectList TotalSongsPerSetlist { get; set; }
    }

    public class MoveSongViewModel
    {
        public int SetlistId { get; set; }
        public string SetlistName { get; set; }
        public SelectList LocationList { get; set; }
    }
}