using System.Collections.Generic;

namespace SetGenerator.WebUI.ViewModels
{
    public class SetlistDetail
    {
        public int Id { get; set; }
        public string Name  { get; set; }
        public int NumSets { get; set; }
        public int NumSongs { get; set; }
        public string Owner { get; set; }
        public string UserUpdate { get; set; }
        public string DateUpdate { get; set; }
    }

    public class SetSongDetail : SongDetail
    {
        public int SetNumber;
    }

    public class SetlistViewModel
    {
        public string BandName { get; set; }
        public string CurrentUser { get; set; }
        public int SelectedId { get; set; }
        public string SelectedOwnerSearch { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }

    public class SetViewModel
    {
        public int SetlistId { get; set; }
        public string Name { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }
}