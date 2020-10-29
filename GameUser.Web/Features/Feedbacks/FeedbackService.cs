using GameUser.Features.Feedbacks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameUser.Web.Features.Feedbacks
{
    public interface IFeedbackServices
    {
        Task<IEnumerable<FeedbackType>> GetTypes();

        Task<bool> AddFeedback(Feedback feedback);
    }

    public class FeedbackServices : IFeedbackServices
    {
        private readonly IFeedbackApi feedbackApi;

        public FeedbackServices(IFeedbackApi feedbackApi)
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
