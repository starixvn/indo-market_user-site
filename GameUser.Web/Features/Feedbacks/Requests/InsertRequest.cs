using GameUser.Web.Features.Feedbacks.ViewModels;

namespace GameUser.Web.Features.Feedbacks.Requests
{
    public class InsertRequest
    {
        public FeedbackViewModel Feedback { get; set; }

        public string NickName { get; set; }

        public int CustId { get; set; }
    }
}