using System.Collections.Generic;

namespace GameUser.Web.Features.APIs.Announcements.ViewModels
{
    public class AnnouncementViewModel
    {
        public Dictionary<string, string> Contents { get; set; }

        public string MessageType { get; set; }

        public bool Status { get; set; }
    }
}