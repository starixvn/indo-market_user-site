using System.ComponentModel.DataAnnotations;

namespace GameUser.Web.Features.Feedbacks.ViewModels
{
    public class AccessUrlRequestViewModel
    {
        [Required]
        public string SessionId { get; set; }
    }
}
