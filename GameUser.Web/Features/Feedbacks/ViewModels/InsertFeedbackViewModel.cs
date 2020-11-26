using GameUser.Features.Feedbacks;

namespace GameUser.Web.Features.Feedbacks.ViewModels
{
    public class InsertFeedbackViewModel
    {
        public Feedback Feedback { get; set; }

        public string NickName { get; set; }

        public int CustId { get; set; }
    }
}
