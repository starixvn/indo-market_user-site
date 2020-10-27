using GameUser.Features.Feedbacks;
using GameUser.Web.Shared;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameUser.Web.Features.Feedbacks
{
    public interface IFeedbackApi : IBaseAuthorizationApi
    {
        [Get("/feedback/get_types")]
        Task<IEnumerable<FeedbackType>> GetTypes();

        [Post("/feedback/add_feedback")]
        Task<bool> AddFeedback(Feedback feedback);
    }
}
