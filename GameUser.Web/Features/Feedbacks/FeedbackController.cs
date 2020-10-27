using GameUser.Features.Feedbacks;
using GameUser.Features.FileUploads;
using GameUser.Features.Mail;
using GameUser.Web.Features.Feedbacks.ViewModels;
using GameUser.Web.Shared.Configurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GameUser.Web.Features.Feedbacks
{
    //[Authorize]
    public class FeedbackController : Controller
    {
        private readonly ISentryClient sentryClient;
        private readonly IFeedbackService feedbackService;
        private readonly ITimeLimitedDataProtector dataProtector;
        private const string dataProtectorKey = "game-round-result-data-key";
        private const int dataProtectorLifeTimeSecond = 1000;
        private readonly IAppSettings appSetting;
        private readonly IFileUploadService fileUploadService;
        private readonly IEmailService emailService;

        public FeedbackController(
            ISentryClient sentryClient,
            IFeedbackService feedbackService,
            IDataProtectionProvider dataProtectionProvider,
            IAppSettings appSetting,
            IFileUploadService fileUploadService,
            IEmailService emailService)
        {
            this.sentryClient = sentryClient;
            this.feedbackService = feedbackService;
            dataProtector = dataProtectionProvider.CreateProtector(dataProtectorKey).ToTimeLimitedDataProtector();
            this.appSetting = appSetting;
            this.fileUploadService = fileUploadService;
            this.emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> AddFeedback()
        {
            var feedbackTypes = await feedbackService.GetTypes();
            var model = new InsertFeedbackViewModel
            {
                Feedback = new GameUser.Features.Feedbacks.Feedback(),
                FeedbackTypeOptions = feedbackTypes.Select(t => new SelectListItem { Text = t.DisplayName, Value = t.Value }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddFeedback(InsertFeedbackViewModel model)
        {
            var result = await feedbackService.AddFeedback(model.Feedback);

            if (result)
            {
                if(model.Feedback.Files != null && model.Feedback.Files.Any())
                {
                    await fileUploadService.UploadFiles(model.Feedback.Files, appSetting.StoreFileFullPath());
                }                

                var email = GenerateEmail(model, "your nickname");
                await emailService.SendAsync(email);
            }

            return Json(new { success = result });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetAccessUrl([FromBody] AccessUrlRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var client = GetClient(model.SessionId);

                if (client != null)
                {
                    var token = dataProtector.Protect($"{model.SessionId}",
                        TimeSpan.FromSeconds(dataProtectorLifeTimeSecond));

                    var redirectUrl = $"{this.appSetting.ResultUrl}/Feedback/Auth/{token}";

                    return Ok(new FeedAccessUrlResultModel { RedirectUrl = redirectUrl });
                }
            }

            return BadRequest();
        }

        [Route("/[controller]/[action]/{token}", Order = 0)]
        [AllowAnonymous]
        public async Task<IActionResult> Auth(string token)
        {
            try
            {
                var sessionId = dataProtector.Unprotect(token);
                var client = GetClient(sessionId);

                if (client != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, client),
                        new Claim(CustomClaimTypes.TimeZone, client)
                    };
                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("AddFeedback");
                }
            }
            catch (Exception exception)
            {
                SentrySdk.CaptureException(exception);
            }

            return BadRequest();
        }

        private EmailRequest GenerateEmail(InsertFeedbackViewModel model, string nickName = null)
        {
            var subject = $"Saba_Club_[{DateTime.Now:dd/MM/yyyy}]_{nickName}_{model.Feedback.Category}";
            var body = $"Dear SB club, " +
                $"{Environment.NewLine}{Environment.NewLine}" +
                $"{model.Feedback.Description} " +
                $"{Environment.NewLine}{Environment.NewLine}" +
                $"This is an automated email. Please do not reply to it.";

            return new EmailRequest
            {
                Subject = subject,
                Body = body,
                Attachments = model.Feedback.Files,
                ToAddress = appSetting.Email.RecieverEmail
            };
        }

        private string GetClient(string session)
        {
            return "ok";
        }
    }
}
