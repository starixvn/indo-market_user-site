using GameUser.Features.Feedbacks;
using GameUser.Web.Features.Feedbacks.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameUser.Web.Features.Feedbacks.Requests
{
    public class InsertRequest
    {
        public FeedbackViewModel Feedback { get; set; }

        public string NickName { get; set; }
    }
}
