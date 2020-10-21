using Microsoft.AspNetCore.Mvc;

namespace UserSite.Api
{
    public class HomeController : Controller
    {
        public IActionResult Index()
            => new ContentResult
            {
                Content = "User Site API is working."
            };
    }
}
