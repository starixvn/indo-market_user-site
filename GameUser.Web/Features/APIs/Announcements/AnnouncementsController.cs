using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameUser.Features.Annoucements;
using GameUser.Features.Announcements;
using GameUser.Shared.SessionFilterAuthentications;
using GameUser.Web.Features.APIs.Announcements.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameUser.Web.Features.APIs.MessageAnnouncement
{
    [ApiController]
    [Route("api/announcement")]
    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementService announcementService;

        public AnnouncementsController(IAnnouncementService announcementService)
        {
            this.announcementService = announcementService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public async Task<JsonResult> Get(string currency = "uus")
        {
            var announcementMessages = await this.announcementService.GetMessages(currency);

            if (announcementMessages != null)
            {
                var result = announcementMessages
                    .Select(x => TransformAnnoucement(x));

                return Json(result);
            }

            return Json(new List<AnnouncementViewModel>());
        }

        private AnnouncementViewModel TransformAnnoucement(AnnouncementEntity annoucementEntity)
        {
            return new AnnouncementViewModel
            {
                Contents = annoucementEntity.Contents,
                MessageType = annoucementEntity.MessageType,
                Status = annoucementEntity.Status
            };
        }
    }
}