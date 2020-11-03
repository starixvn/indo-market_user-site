using GameUser.Features.Emails;
using GameUser.Features.Feedbacks;
using GameUser.Features.FileUploads;
using GameUser.Web.Features.Feedbacks.ViewModels;
using GameUser.Web.Shared;
using GameUser.Web.Shared.Configurations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sentry;
using System.Linq;
using System.Threading.Tasks;

namespace GameUser.Web.Features.Feedbacks
{
    [UserSessionValidation]
    [AutoValidateAntiforgeryToken]
    public class FeedbackController : Controller
    {        
        private readonly IFeedbackService feedbackService;                    
        private readonly IAppSettings appSetting;
        private readonly IEmailConfigs emailConfigs;
        private readonly IFileUploadService fileUploadService;
        private readonly IEmailService emailService;

        public FeedbackController(            
            IFeedbackService feedbackService,            
            IAppSettings appSetting,
            IFileUploadService fileUploadService,
            IEmailService emailService,
            IEmailConfigs emailConfigs)
        {            
            this.feedbackService = feedbackService;            
            this.appSetting = appSetting;
            this.fileUploadService = fileUploadService;
            this.emailService = emailService;
            this.emailConfigs = emailConfigs;
        }
       
        [HttpGet]        
        public async Task<IActionResult> Add(FeedbackRequest feedbackRequest)
        {
            var feedbackTypes = await feedbackService.GetTypes();
            var model = new InsertFeedbackViewModel
            {
                Feedback = new GameUser.Features.Feedbacks.Feedback(),
                FeedbackTypeOptions = feedbackTypes.Select(t => new SelectListItem { Text = t.DisplayName, Value = t.Value }).ToList(),
                NickName = feedbackRequest.NickName,
                SessionId = feedbackRequest.SessionId
            };

            return View(model);
        }

        [HttpPost]        
        public async Task<IActionResult> Add(InsertFeedbackViewModel model)
        {
            var result = await feedbackService.AddFeedbackAsync(model.Feedback);

            if (result.IsSuccess)
            {
                if(model.Feedback.Files != null && model.Feedback.Files.Any())
                {
                    await fileUploadService.UploadFilesAsync(model.Feedback.Files, appSetting.StoreFileFullPath());
                }                

                var email = await feedbackService.GenerateEmail(model.Feedback, model.NickName, emailConfigs);

                await emailService.SendAsync(email);
            }

            return Json(result);
        }             
    }
}
