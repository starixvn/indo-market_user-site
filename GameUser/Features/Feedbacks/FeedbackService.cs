using GameUser.Features.Emails;
using GameUser.Shared.Enumerations;
using GameUser.Shared.Repositories;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameUser.Features.Feedbacks
{
    public interface IFeedbackService
    {
        Task<IEnumerable<FeedbackType>> GetTypes();
        Task<bool> AddFeedback(Feedback feedback);

        Task<EmailRequest> GenerateEmail(Feedback feedback, string nickname, IEmailConfigs emailConfigs);
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
            catch (Exception e)
            {
                sentryClient.CaptureException(e);
                return false;
            }
        }

        public async Task<EmailRequest> GenerateEmail(Feedback feedback, string nickname, IEmailConfigs emailConfigs)
        {
            var feedbackTypes = await GetTypes();

            var category = feedbackTypes.FirstOrDefault(x => x.Value == feedback.Category).DisplayName;

            var subject = $"Saba_Club_[{DateTime.Now:dd/MM/yyyy}]_{nickname}_{category}";
            var body = $"Dear SB club, " +
                $"{Environment.NewLine}{Environment.NewLine}" +
                $"{feedback.Description} " +
                $"{Environment.NewLine}{Environment.NewLine}" +
                $"This is an automated email. Please do not reply to it.";

            return new EmailRequest
            {
                Subject = subject,
                Body = body,
                Attachments = feedback.Files,
                ToAddress = emailConfigs.RecieverEmail
            };
        }

        public Task<IEnumerable<FeedbackType>> GetTypes()
        {
            return (Task<IEnumerable<FeedbackType>>)Enumeration.GetAll<FeedbackType>();
        }
    }
}
