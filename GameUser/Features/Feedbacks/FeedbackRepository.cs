using GameUser.Shared.Repositories;
using MongoDB.Driver;

namespace GameUser.Features.Feedbacks
{
    public interface IFeedbackRepository: IGenericRepository<FeedbackEntity>
    {

    }

    public class FeedbackRepository: GenericRepository<FeedbackEntity>, IFeedbackRepository
    {
        public FeedbackRepository(IMongoClient client): base(client.GetDatabase("member"))
        {

        }
    }
}
