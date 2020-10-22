using System;

namespace GameUser.Features.Feedbacks
{
    public class Feedback
    {
        public DateTime Time { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; }
    }
}
