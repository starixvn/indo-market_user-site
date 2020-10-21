namespace UserSite.Core.Enumeration
{
    public class FeedbackType : Enumeration
    {
        public const string NewGameValue = "newgame";
        public const string ReportBugValue = "reportbug";
        public const string OtherIssuesValue = "otherissues";

        public static readonly FeedbackType NewGame = new FeedbackType(NewGameValue, "Request a new game");
        public static readonly FeedbackType ReportBug = new FeedbackType(ReportBugValue, "Report a bug");
        public static readonly FeedbackType OtherIssues = new FeedbackType(OtherIssuesValue, "Report other issues");

        public FeedbackType()
        {
        }

        public FeedbackType(string value, string displayName)
            : base(value, displayName)
        {
        }
    }
}
