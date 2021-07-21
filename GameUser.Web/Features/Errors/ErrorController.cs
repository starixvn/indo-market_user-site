using Microsoft.AspNetCore.Mvc;

namespace GameUser.Web.Features.Errors
{
    public class ErrorController : Controller
    {
        public IActionResult NotAuthorized()
        {
            return View();
        }
    }
}