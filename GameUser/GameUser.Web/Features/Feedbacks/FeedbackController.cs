using Microsoft.AspNetCore.Mvc;
using Sentry;
using System.Threading.Tasks;

namespace GameUser.Web.Features.Feedbacks
{
    public class FeedbackController : Controller
    {
        private readonly ISentryClient sentryClient;
        private readonly IFeedbackService feedbackService;

        public FeedbackController(ISentryClient sentryClient, IFeedbackService feedbackService)
        {
            this.sentryClient = sentryClient;
            this.feedbackService = feedbackService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTypes()
        {
            var configs = await feedbackService.GetTypes();

            return Ok(configs);
        }
    }
}
