using GameUser.Api.Shared.Authorizes;
using GameUser.Features.Feedbacks;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GameUser.Api.Features.Feedbacks
{
    [Route("api/feedback")]
    public class FeedbackController : BaseAuthorizeController
    {
        private readonly IFeedbackService feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            this.feedbackService = feedbackService;
        }

        [HttpGet("get_types")]
        public async Task<IActionResult> GetTypes()
        {
            var configs = await feedbackService.GetTypes();

            return Ok(configs);
        }

        [HttpPost("add_feedback")]
        public async Task<IActionResult> AddFeedback(Feedback feedback)
        {
            var configs = await feedbackService.AddFeedback(feedback);

            return Ok(configs);
        }
    }
}
