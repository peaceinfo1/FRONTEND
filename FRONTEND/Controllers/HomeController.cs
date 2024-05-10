using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FRONTEND.Models;
using Org.BouncyCastle.Asn1.Cms;
using BOL.AUDITTRAIL;
using DAL.LISTING;
using DAL.AUDIT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using HUBS.Notifications;
using StringRandomizer;
using StringRandomizer.Stores;
using StringRandomizer.Options;
using Microsoft.AspNetCore.Http;

namespace FRONTEND.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ListingDbContext listingContext;
        private readonly AuditDbContext auditContext;
        private readonly IHubContext<NotificationUserDashboardHub> notificationUserDashboardHub;

        public HomeController(ILogger<HomeController> logger, ListingDbContext listingContext, AuditDbContext auditContext, IHubContext<NotificationUserDashboardHub> notificationUserDashboardHub)
        {
            _logger = logger;
            this.listingContext = listingContext;
            this.auditContext = auditContext;
            this.notificationUserDashboardHub = notificationUserDashboardHub;
        }

        [HttpGet]
        [Route("/url/{digits}")]
        public string url(int digits)
        {
            var randomizer = new Randomizer(digits, new DefaultRandomizerOptions(hasNumbers:true, hasLowerAlphabets: true, hasUpperAlphabets: false), new DefaultRandomizerStore());
            var result = randomizer.Next();

            string url = $"myinteriormart.com/{result}";

            return url;
        }

        public IActionResult Index()
        {
            HttpContext.Session.SetString("mobile", "8976643925");

            return View();
        }

        public IActionResult About()
        {
            HttpContext.Session.SetString("mobile", "8976643925");

            return View();
        }
        public IActionResult TermsOfUse()
        {
            HttpContext.Session.SetString("mobile", "8976643925");

            return View();
        }

        [Authorize]
        public IActionResult Test()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact([Bind("ContactId,Name,Email,Mobile,Message,City,Date,IPAddress")] Contact contact)
        {
            if(contact.Name == null)
            {
                TempData["Message"] = $"Dear user, please add your full name.";
            }

            if (contact.Email == null)
            {
                TempData["Message"] = $"Dear {contact.Name}, please add your email address.";
            }

            if (contact.Mobile.ToString() == "")
            {
                TempData["Message"] = $"Dear {contact.Name}, please add your mobile number.";
            }

            if (contact.Message == null)
            {
                TempData["Message"] = $"Dear {contact.Name}, please add your message.";
            }

            if (ModelState.IsValid)
            {
                auditContext.Add(contact);
                await auditContext.SaveChangesAsync();

                TempData["Message"] = $"Dear {contact.Name}, thank you for contact us. We will get back to you soon.";

                return RedirectToAction("Success");
            }

            return View(contact);
        }

        public IActionResult AccountSuspended()
        {
            return View();
        }

        public IActionResult AccountUnsuspended()
        {
            return View();
        }

        [HttpGet]
        public string Shafi(string firstName, string lastName, int age)
        {
            string result = "First Name: " + firstName + " Last Name: " + lastName + "Age: " + age;
            return result;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
