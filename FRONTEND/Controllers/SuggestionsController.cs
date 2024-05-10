using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BOL.AUDITTRAIL;
using DAL.AUDIT;
using BAL.Messaging.Contracts;
using Microsoft.AspNetCore.Hosting;
using System.Text;

namespace FRONTEND.Controllers
{
    public class SuggestionsController : Controller
    {
        private readonly AuditDbContext _context;
        private readonly IMessageMailService notificationRepo;
        private readonly IWebHostEnvironment webHost;

        public SuggestionsController(AuditDbContext context, IMessageMailService notificationRepo, IWebHostEnvironment webHost)
        {
            _context = context;
            this.notificationRepo = notificationRepo;
            this.webHost = webHost;
        }

        // GET: Suggestions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Suggestions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SuggestionID,Date,Name,Email,Mobile,Suggestion")] Suggestions suggestions)
        {
            if (ModelState.IsValid)
            {
                _context.Add(suggestions);
                await _context.SaveChangesAsync();

                // Shafi: Send notification to suggester
                string mobile = suggestions.Mobile;
                string smsMessage = $"Dear {suggestions.Name}, thank you for submitting your valuable suggestion for www.myinteriormart.com we will let you know when we implement your great advice in our organisation.";
                string email = suggestions.Email;
                string subject = "Thank You For Your Suggestion";

                // Email Template
                var webRoot = webHost.WebRootPath;
                var emailTemplate = System.IO.Path.Combine(webRoot, "Email-Templates", "Suggestions-Notifications.html");
                string emailMessage = System.IO.File.ReadAllText(emailTemplate, Encoding.UTF8).Replace("{suggestion}", suggestions.Suggestion).Replace("{name}", suggestions.Name);
                // End:

                notificationRepo.SendBoth(mobile, smsMessage, email, subject, emailMessage);
                // End:

                return RedirectToAction("Success", "Home");
            }
            return View(suggestions);
        }
    }
}
