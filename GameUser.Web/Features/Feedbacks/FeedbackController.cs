using System.Linq;
using System.Threading.Tasks;
using GameUser.Features.Avatars;
using GameUser.Features.Feedbacks;
using GameUser.Features.UserInfos;
using GameUser.Features.UserSessions;
using GameUser.Shared.Models;
using GameUser.Web.Features.Feedbacks.Requests;
using GameUser.Web.Features.Feedbacks.ViewModels;
using GameUser.Web.Shared.Attributes;
using GameUser.Web.Shared.Configurations;
using GameUser.Web.Shared.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

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
        private readonly IAvatarService avatarService;
        private readonly IUserSessionService userSessionService;
        private readonly IStringLocalizer<Resources> localizer;
        private readonly IUserInfoService userInfoService;

        public FeedbackController(
            IHttpContextAccessor httpContext,
            IFeedbackService feedbackService,
            IAppSettings appSetting,
            IAvatarService avatarService,
            IUserSessionService userSessionService,
            IStringLocalizer<Resources> localizer,
            IUserInfoService userInfoService)
        {
            this.httpContext = httpContext;
            this.feedbackService = feedbackService;
            this.appSetting = appSetting;
            this.avatarService = avatarService;
            this.userSessionService = userSessionService;
            this.localizer = localizer;
            this.userInfoService = userInfoService;
        }

        [HttpGet]
        public async Task<IActionResult> Add(GetRequest feedbackRequest)
        {
            var feedbackTypes = await feedbackService.GetTypes();
            var sessionId = feedbackRequest.SessionId ?? this.httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "sessionid").Value;
            var userSession = await userSessionService.GetUserSession(sessionId);
            var userInfo = await userInfoService.GetUserInfo(userSession.UserId);

            var model = new GetFeedbackViewModel
            {
                Feedback = new FeedbackViewModel(),
                FeedbackTypeOptions = feedbackTypes.ToList(),
                NickName = userInfo.Name,
                Culture = (string)httpContext.HttpContext.Request.RouteValues["culture"] ?? "en-US",
                Avatar = await avatarService.GetAvatar(userInfo.AvatarId.ToString()),
                CustId = userSession.MemberId
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(InsertRequest addRequest)
        {
            if (ModelState.IsValid)
            {
                var viewModel = new InsertFeedbackViewModel
                {
                    Feedback = new Feedback(
                    category: addRequest.Feedback.Category,
                    description: addRequest.Feedback.Description.Trim(),
                    files: addRequest.Feedback.Files),
                    NickName = addRequest.NickName,
                    CustId = addRequest.CustId
                };

                var result = await feedbackService.AddFeedbackAsync(
                    viewModel.Feedback,
                    viewModel.NickName,
                    appSetting.EnabledStoreFile,
                    appSetting.StoreFileFullPath(),
                    viewModel.CustId,
                    appSetting.Env);

                return Json(result);
            }

            return Json(new UpdateResult(false, this.localizer.GetString("Something_Wrong_On_Attachment")));
        }

        [HttpGet]
        public ActionResult RefreshToken()
        {
            return PartialView("_AntiForgeryToken");
        }
    }
}