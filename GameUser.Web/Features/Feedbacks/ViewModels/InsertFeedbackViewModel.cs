using GameUser.Features.Feedbacks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace GameUser.Web.Features.Feedbacks.ViewModels
{
    public class InsertFeedbackViewModel
    {
        public InsertFeedbackViewModel()
        {
            FeedbackTypeOptions = new List<SelectListItem>
            {
                new SelectListItem {Text = "type 1", Value = "typ1"},
                new SelectListItem {Text = "type 2", Value = "typ2"},
            };
        }

        public List<SelectListItem> FeedbackTypeOptions { get; set; }

        public Feedback Feedback { get; set; }

        public string NickName { get; set; }

        public string SessionId { get; set; }

        public string Culture { get; set; }
    }
}
