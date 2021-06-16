using System.Collections.Generic;
using GameUser.Features.Avatars;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GameUser.Web.Features.Feedbacks.ViewModels
{
    public class GetFeedbackViewModel
    {
        public List<SelectListItem> FeedbackTypeOptions { get; set; }

        public FeedbackViewModel Feedback { get; set; }

        public string NickName { get; set; }

        public string Culture { get; set; }

        public AvatarObject Avatar { get; set; }

        public int CustId { get; set; }
    }
}