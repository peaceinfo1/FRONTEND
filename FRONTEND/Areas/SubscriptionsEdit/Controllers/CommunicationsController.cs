using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BOL.LISTING;
using DAL.LISTING;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using BAL.Audit;
using BAL.Listings;
using BAL.Services.Contracts;
using DAL.SHARED;

namespace FRONTEND.Areas.SubscriptionsEdit.Controllers
{
    [Area("SubscriptionsEdit")]
    public class CommunicationsController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly SharedDbContext sharedManager;
        private readonly IUserService _userService;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;

        public CommunicationsController(ListingDbContext listingContext, IUserService userService, IHistoryAudit audit, IListingManager listingManager, SharedDbContext sharedManager)
        {
            this.listingContext = listingContext;
            this.sharedManager = sharedManager;
            this._userService = userService;
            this.listingManager = listingManager;
            this.audit = audit;
        }

        // GET: SubscriptionsEdit/Communications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewData["Language"] = sharedManager.Languages.Select(l => l.Name).ToList();
            if (id == null)
            {
                return NotFound();
            }

            var communication = await listingContext.Communication
                .FirstOrDefaultAsync(m => m.ListingID == id);
            if (communication == null)
            {
                return NotFound();
            }

            return View(communication);
        }

        // GET: SubscriptionsEdit/Communications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewData["Language"] = sharedManager.Languages.Select(l => l.Name).ToList();
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string OwnerGuid = user.Id;
            // End:

            if (id == null)
            {
                return NotFound();
            }

            var communication = await listingContext.Communication.FindAsync(id);
            if (communication == null)
            {
                return NotFound();
            }

            // Shafi: Verify record ownership
            var owner = await listingManager.CommunicationOwnerAsync(communication.CommunicationID, communication.ListingID, OwnerGuid);
            if(owner == true)
            {
                return View(communication);
            }
            else
            {
                return NotFound();
            }
            // End:
        }

        // POST: SubscriptionsEdit/Communications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CommunicationID,ListingID,OwnerGuid,IPAddress,Language,Mobile,TollFree,Email,Telephone,TelephoneSecond,Website,SkypeID")] Communication communication)
        {
            ViewData["Language"] = sharedManager.Languages.Select(l => l.Name).ToList();
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string userGuid = user.Id;
            // End:

            if (id != communication.CommunicationID)
            {
                return NotFound();
            }

            // Shafi: Verify ownership
            var owner = await listingManager.CommunicationOwnerAsync(communication.CommunicationID, communication.ListingID, userGuid);
            if (owner == true)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        listingContext.Update(communication);
                        await listingContext.SaveChangesAsync();
                        // Shafi: Create last updated
                        string ipAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
                        string userAgent = this.HttpContext.Request.Headers["User-Agent"];
                        string referUrl = this.HttpContext.Request.Headers["Referer"];
                        string visitedURL = this.HttpContext.Request.Headers["Host"];
                        string email = user.Email;
                        string mobile = user.PhoneNumber;
                        DateTime timeZoneDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        string updatedDate = timeZoneDate.ToString("d-MM-yyyy");
                        string updatedTime = timeZoneDate.ToString("hh:mm:ss tt");
                        string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/Subscriptions/Communications/Edit/" + communication.CommunicationID;
                        string activity = "Updated communication details with id " + communication.CommunicationID;

                        // Shafi: Get user in roles
                        IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                        string roleName = userInRoleName.FirstOrDefault();
                        // End:

                        await audit.CreateListingLastUpdatedAsync(communication.CommunicationID, userGuid, email, mobile, ipAddress, roleName, "Communication", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                        // End:
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CommunicationExists(communication.CommunicationID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    // Shafi: Show success message in redirected view
                    TempData["SuccessMessage"] = "Communication details saved successfully.";
                    // End:

                    return Redirect("/SubscriptionsEdit/Communications/Details/" + id);
                }
            }
            else
            {
                return NotFound();
            }

            return View(communication);
        }

        private bool CommunicationExists(int id)
        {
            return listingContext.Communication.Any(e => e.CommunicationID == id);
        }
    }
}
