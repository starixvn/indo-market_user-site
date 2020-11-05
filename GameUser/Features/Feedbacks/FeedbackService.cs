using GameUser.Features.Emails;
using GameUser.Shared.Enumerations;
using GameUser.Shared.Models;
using GameUser.Shared.Repositories;
using GameUser.Shared.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameUser.Features.Feedbacks
{
    public interface IFeedbackService
    {
        Task<IEnumerable<SelectListItem>> GetTypes();
        Task<UpdateResult> AddFeedbackAsync(Feedback feedback);

        Task<EmailRequest> GenerateEmail(Feedback feedback, string nickname, IEmailConfigs emailConfigs);
    }

    public class FeedbackService : IFeedbackService
    {
        private readonly IGenericRepository<FeedbackEntity> feedbackRepository;
        private readonly ISentryClient sentryClient;
        private readonly IFeedbackTypeLocalization feedbackTypeLocalization;
        private readonly IStringLocalizer<Resources> localizer;

        public FeedbackService(
            IGenericRepository<FeedbackEntity> feedbackRepository, 
            ISentryClient sentryClient,
            IFeedbackTypeLocalization feedbackTypeLocalization,
            IStringLocalizer<Resources> localizer)
        {
            this.feedbackRepository = feedbackRepository;
            this.sentryClient = sentryClient;
            this.feedbackTypeLocalization = feedbackTypeLocalization;
            this.localizer = localizer;
        }

        public async Task<UpdateResult> AddFeedbackAsync(Feedback feedback)
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

                return new UpdateResult { IsSuccess = true };

            }
            catch (Exception e)
            {
                sentryClient.CaptureException(e);
                return new UpdateResult { IsSuccess = false, Message = localizer["Something_Wrong"] };
            }
        }

        public async Task<EmailRequest> GenerateEmail(Feedback feedback, string nickname, IEmailConfigs emailConfigs)
        {
            var feedbackTypes = await GetTypes();

            var category = feedbackTypes.FirstOrDefault(x => x.Value == feedback.Category).Text;

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

        public async Task<IEnumerable<SelectListItem>> GetTypes()
        {
            return await Task.FromResult(feedbackTypeLocalization.GetSelectListItems());
        }
    }
}
