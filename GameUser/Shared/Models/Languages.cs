namespace GameUser.Shared.Models
{
    public static class Languages
    {
        private static readonly string defaultLanguage = "en";

        public static string Convert(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return defaultLanguage;
            }

            switch (language)
            {
                case "en-US":
                case "en":
                    return "en";
                case "id-ID":
                case "id":
                    return "id";
                case "th-TH":
                case "th":
                    return "th";
                case "ko-KR":
                case "ko":
                    return "ko";
                case "zh-CN":
                case "zhCN":
                    return "zhCN";
                case "zh-Hans":
                case "zhHans":
                    return "zhHans";
                case "zh-TW":
                case "zhTW":
                    return "zhTW";
                default:
                    return defaultLanguage;
            }
        }
    }
}