using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GameUser.Web.Features.Feedbacks.ViewModels
{
    public class GetFeedbackViewModel
    {
        public List<SelectListItem> FeedbackTypeOptions { get; set; }

        public FeedbackViewModel Feedback { get; set; }

        public string NickName { get; set; }

        public string Culture { get; set; }

        public int AvatarId { get; set; }

        public int CustId { get; set; }
    }
}