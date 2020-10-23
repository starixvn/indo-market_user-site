using GameUser.Web.Features.Feedbacks.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sentry;
using System.Linq;
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

        [HttpGet]
        public async Task<IActionResult> AddFeedback()
        {
            var feedbackTypes = await feedbackService.GetTypes();
            var model = new InsertFeedbackViewModel { 
                Feedback = new GameUser.Features.Feedbacks.Feedback(),
                FeedbackTypeOptions = feedbackTypes.Select(t => new SelectListItem { Text = t.DisplayName, Value = t.Value}).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddFeedback(InsertFeedbackViewModel model)
        {
            var result = await feedbackService.AddFeedback(model.Feedback);

            return Json(new { success = result});
        }
    }
}
