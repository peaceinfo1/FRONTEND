using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FRONTEND.Models;

namespace FRONTEND.Controllers
{
    public class AjaxT : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Me()
        {
            return new JsonResult("Great! Ajax is working.");
        }

        [HttpGet]
        public IActionResult GetProduct()
        {
            string p = "product ID: ";

            return new JsonResult(p);
        }
    }
}
