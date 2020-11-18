using GameUser.Features.Avatars;
using GameUser.Features.Feedbacks;
using GameUser.Features.UserSessions;
using GameUser.Web.Features.Feedbacks.ViewModels;
using GameUser.Web.Shared.Attributes;
using GameUser.Web.Shared.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IAvatarService avatarService;
        private readonly IUserSessionService userSessionService;

        public FeedbackController(
            IHttpContextAccessor httpContext,
            IFeedbackService feedbackService,            
            IAppSettings appSetting,
            IAvatarService avatarService,
            IUserSessionService userSessionService)
        {
            this.httpContext = httpContext;
            this.feedbackService = feedbackService;            
            this.appSetting = appSetting;
            this.avatarService = avatarService;
            this.userSessionService = userSessionService;
        }

        [HttpGet]        
        public async Task<IActionResult> Add(FeedbackRequest feedbackRequest)
        {
            var feedbackTypes = await feedbackService.GetTypes();
            var userInfo = await userSessionService.GetUserInfo(feedbackRequest.SessionId);
            var model = new InsertFeedbackViewModel
            {
                Feedback = new Feedback(),
                FeedbackTypeOptions = feedbackTypes.ToList(),
                NickName = userInfo.Name,
                SessionId = feedbackRequest.SessionId,
                Culture = (string)httpContext.HttpContext.Request.RouteValues["culture"] ?? "en",
                Avatar = await avatarService.GetAvatar(userInfo.AvatarId.ToString())
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(InsertFeedbackViewModel model)
        {
            var result = await feedbackService.AddFeedbackAsync(model.Feedback, model.NickName, appSetting.EnabledStoreFile, appSetting.StoreFileFullPath());            

            return Json(result);
        }

        [HttpGet]
        public ActionResult RefreshToken()
        {
            return PartialView("_AntiForgeryToken");
        }
    }
}