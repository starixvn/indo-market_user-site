using GameUser.Features.Feedbacks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameUser.Web.Features.Feedbacks
{
    public interface IFeedbackService
    {
        Task<IEnumerable<FeedbackType>> GetTypes();
        Task<bool> AddFeedback(Feedback feedback);
    }

    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackApi feedbackApi;

        public FeedbackService(IFeedbackApi feedbackApi)
        {
            this.feedbackApi = feedbackApi;
        }

        public Task<bool> AddFeedback(Feedback feedback)
        {
            return feedbackApi.AddFeedback(feedback);
        }

        public async Task<IEnumerable<FeedbackType>> GetTypes()
        {
            return await feedbackApi.GetTypes();
        }
    }
}
