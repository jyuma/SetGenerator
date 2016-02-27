using System.Collections.Generic;

namespace SetGenerator.WebUI.ViewModels
{
    public class InstrumentDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public bool IsSongInstrument { get; set; }
    }

    public class InstrumentViewModel
    {
        public int SelectedId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }

    public class InstrumentEditViewModel
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
    }
}
