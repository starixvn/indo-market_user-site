using GameUser.Features.Feedbacks;
using GameUser.Shared.Enumerations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameUser.Web.Features.Feedbacks
{
    public interface IFeedbackService
    {
        Task<IEnumerable<FeedbackType>> GetTypes();
    }

    public class FeedbackService : IFeedbackService
    {
        public async Task<IEnumerable<FeedbackType>> GetTypes()
        {
            return Enumeration.GetAll<FeedbackType>();
        }
    }
}
