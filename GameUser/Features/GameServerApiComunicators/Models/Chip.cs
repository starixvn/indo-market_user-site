namespace GameUser.Features.GameServerApiComunicators.Models
{
    public class ChipTheme
    {
        public string BackgroundColor { get; set; }

        public string BorderColor { get; set; }

        public string CenterColor { get; set; }

        public string LabelColor { get; set; }
    }

    public class Chip
    {
        public string Label { get; set; }

        public string Value { get; set; }

        public ChipTheme Theme { get; set; }
    }
}