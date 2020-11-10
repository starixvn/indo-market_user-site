using GameUser.Features.Emails;
using GameUser.Features.Feedbacks;
using GameUser.Features.FileUploads;
using GameUser.Shared.Models;
using GameUser.Web.Features.Feedbacks.ViewModels;
using GameUser.Web.Shared.Attributes;
using GameUser.Web.Shared.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sentry;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameUser.Web.Features.Feedbacks
{
    [UserSessionValidation]
    [AutoValidateAntiforgeryToken]
    [Internationalization]
    public class FeedbackController : Controller
    {
        private readonly IHttpContextAccessor httpContext;
        private readonly IFeedbackService feedbackService;                    
        private readonly IAppSettings appSetting;
        private readonly IEmailConfigs emailConfigs;
        private readonly IFileUploadService fileUploadService;
        private readonly IEmailService emailService;
        private readonly ISentryClient sentryClient;

        public FeedbackController(
            IHttpContextAccessor httpContext,
            IFeedbackService feedbackService,            
            IAppSettings appSetting,
            IFileUploadService fileUploadService,
            IEmailService emailService,
            IEmailConfigs emailConfigs,
            ISentryClient sentryClient)
        {
            this.httpContext = httpContext;
            this.feedbackService = feedbackService;            
            this.appSetting = appSetting;
            this.fileUploadService = fileUploadService;
            this.emailService = emailService;
            this.emailConfigs = emailConfigs;
            this.sentryClient = sentryClient;
        }

        [HttpGet]
        public async Task<IActionResult> Auth(FeedbackRequest feedbackRequest)
        {
            return await Task.Run<ActionResult>(() => RedirectToAction("Add", feedbackRequest));
        }

        [HttpGet]        
        public async Task<IActionResult> Add(FeedbackRequest feedbackRequest)
        {
            var feedbackTypes = await feedbackService.GetTypes();
            var model = new InsertFeedbackViewModel
            {
                Feedback = new Feedback(),
                FeedbackTypeOptions = feedbackTypes.ToList(),
                NickName = feedbackRequest.NickName,
                SessionId = feedbackRequest.SessionId,
                Culture = (string)httpContext.HttpContext.Request.RouteValues["culture"] ?? "en"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(InsertFeedbackViewModel model)
        {
            var result = new UpdateResult { IsSuccess = true};
            try
            {
                await feedbackService.AddFeedbackAsync(model.Feedback);
                await fileUploadService.UploadFilesAsync(model.Feedback.Files, appSetting.StoreFileFullPath());
                var email = await feedbackService.GenerateEmail(model.Feedback, model.NickName, emailConfigs);
                await emailService.SendAsync(email);
            }
            catch (Exception e) {
                sentryClient.CaptureException(e);
                result = new UpdateResult { IsSuccess = false, Message = "Something went wrong" };
            }

            return Json(result);
        }             
    }
}