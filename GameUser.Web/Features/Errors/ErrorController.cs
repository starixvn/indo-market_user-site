using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
