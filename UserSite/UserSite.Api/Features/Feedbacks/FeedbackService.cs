using Sentry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserSite.Core.Enumeration;
using UserSite.Core.Models.Feedback;
using UserSite.Database.Entities.Feedback;
using UserSite.Database.Repositories;

namespace UserSite.Api.Features.Feedbacks
{
    public interface IFeedbackService
    {
        Task<IEnumerable<FeedbackType>> GetTypes();
        Task<bool> AddFeedback(Feedback feedback);
    }

    public class FeedbackService : IFeedbackService
    {
        private readonly IGenericRepository<FeedbackEntity> feedbackRepository;
        private readonly ISentryClient sentryClient;

        public FeedbackService(IGenericRepository<FeedbackEntity> feedbackRepository, ISentryClient sentryClient)
        {
            this.feedbackRepository = feedbackRepository;
            this.sentryClient = sentryClient;
        }

        public async Task<bool> AddFeedback(Feedback feedback)
        {
            try
            {
                var feedbackEntity = new FeedbackEntity
                {
                    Time = DateTime.Now,
                    Category = feedback.Category,
                    Description = feedback.Description,
                    MemberId = feedback.MemberId,
                    MemberName = feedback.MemberName
                };

                await feedbackRepository.InsertOneAsync(feedbackEntity);
                return true;
            }
            catch(Exception e)
            {
                sentryClient.CaptureException(e);
                return false;
            }
        }

        public async Task<IEnumerable<FeedbackType>> GetTypes()
        {
            return Enumeration.GetAll<FeedbackType>();
        }
    }
}