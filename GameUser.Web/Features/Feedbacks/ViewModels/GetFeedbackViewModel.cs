using GameUser.Features.Avatars;
using GameUser.Features.Feedbacks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace GameUser.Web.Features.Feedbacks.ViewModels
{
    public class GetFeedbackViewModel
    {
        public List<SelectListItem> FeedbackTypeOptions { get; set; }

        public FeedbackViewModel Feedback { get; set; }

        public string NickName { get; set; }

        public string Culture { get; set; }

        public AvatarObject Avatar { get; set; }
    }
}
