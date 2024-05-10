using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
    [Authorize]
    public class ErrorsAndExceptionsController : Controller
    {
        public IActionResult ImageFormatNotSupported()
        {
            return View();
        }

        public IActionResult FreeUsersCanCreate5Listings()
        {
            return View();
        }
    }
}
