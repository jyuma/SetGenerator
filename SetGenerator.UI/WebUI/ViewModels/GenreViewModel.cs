using System.Collections.Generic;

namespace SetGenerator.WebUI.ViewModels
{
    public class GenreDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSongGenre { get; set; }
    }

    public class GenreViewModel
    {
        public int SelectedId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }

    public class GenreEditViewModel
    {
        public string Name { get; set; }
    }
}
