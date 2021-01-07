namespace GameUser.Shared.Models
{
    public static class Languages
    {
        private static string defaultLanguage = "en";

        public static string Convert(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return defaultLanguage;
            }

            switch (language)
            {
                case "en-US":
                    return "en";
                case "id-ID":
                    return "id";
                case "th-TH":
                    return "th";
                default:
                    return defaultLanguage;
            }
        }
    }
}
