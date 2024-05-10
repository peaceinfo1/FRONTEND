using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    public class ListingManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
