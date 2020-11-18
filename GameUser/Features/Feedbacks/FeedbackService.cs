using GameUser.Features.Emails;
using GameUser.Features.FileUploads;
using GameUser.Shared.Database;
using GameUser.Shared.Enumerations;
using GameUser.Shared.Models;
using GameUser.Shared.Resources;
using Microsoft.AspNetCore.Http;
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
        Task<UpdateResult> AddFeedbackAsync(Feedback feedback, string nickName, bool enabledStoreFile, string filePath);
    }

    public class FeedbackService : IFeedbackService
    {
        private readonly IGenericRepository<FeedbackEntity> feedbackRepository;
        private readonly ISentryClient sentryClient;
        private readonly IFeedbackTypeLocalization feedbackTypeLocalization;
        private readonly IStringLocalizer<Resources> localizer;
        private readonly IEmailConfigs emailConfigs;
        private readonly IFileUploadService fileUploadService;
        private readonly IEmailService emailService;

        public FeedbackService(
            IGenericRepository<FeedbackEntity> feedbackRepository, 
            ISentryClient sentryClient,
            IFeedbackTypeLocalization feedbackTypeLocalization,
            IStringLocalizer<Resources> localizer,
            IEmailConfigs emailConfigs,
            IFileUploadService fileUploadService,
            IEmailService emailService)
        {
            this.feedbackRepository = feedbackRepository;
            this.sentryClient = sentryClient;
            this.feedbackTypeLocalization = feedbackTypeLocalization;
            this.localizer = localizer;
            this.emailConfigs = emailConfigs;
            this.fileUploadService = fileUploadService;
            this.emailService = emailService;
        }

        public async Task<IEnumerable<SelectListItem>> GetTypes()
        {
            return await Task.FromResult(feedbackTypeLocalization.GetSelectListItems());
        }

        public async Task<UpdateResult> AddFeedbackAsync(Feedback feedback, string nickName, bool enabledStoreFile, string filePath)
        {
            try
            {
                await SaveFeedbackAsync(feedback);
                await StoreFileAsync(feedback.Files, enabledStoreFile, filePath);               
                var email = await GenerateEmail(feedback, nickName);
                await emailService.SendAsync(email);

                return new UpdateResult(isSuccess: true, message: string.Empty);
            }
            catch (Exception e)
            {
                sentryClient.CaptureException(e);
                return new UpdateResult(isSuccess: false, message: localizer["Something_Wrong"]);
            }
        }

        private async Task StoreFileAsync(List<IFormFile> files, bool enabledStoreFile, string filePath)
        {
            if (enabledStoreFile)
            {
                await fileUploadService.UploadFilesAsync(files, filePath);
            }
        }

        private async Task SaveFeedbackAsync(Feedback feedback)
        {           
            var feedbackEntity = new FeedbackEntity
            {
                Time = DateTime.Now,
                Category = feedback.Category,
                Description = feedback.Description,
                Nickname = feedback.Nickname
            };

            await feedbackRepository.InsertOneAsync(feedbackEntity);
        }

        private async Task<EmailRequest> GenerateEmail(Feedback feedback, string nickname)
        {
            var feedbackTypes = await GetTypes();

            var category = feedbackTypes.FirstOrDefault(x => x.Value == feedback.Category).Text;

            var subject = $"Saba_Club_[{DateTime.Now:dd/MM/yyyy}]_{nickname}_{category}";
            var body = $"Dear SB club, " +
                $"{Environment.NewLine}{Environment.NewLine}" +
                $"{feedback.Description} " +
                $"{Environment.NewLine}{Environment.NewLine}" +
                $"This is an automated email. Please do not reply to it.";

            return new EmailRequest(subject: subject, body: body, attachments: feedback.Files, toAddress: emailConfigs.ReceiverEmail);            
        }
    }
}
