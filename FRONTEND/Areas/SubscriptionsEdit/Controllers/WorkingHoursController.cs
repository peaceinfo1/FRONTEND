using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BOL.LISTING;
using DAL.LISTING;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Http;
using DAL.SHARED;
using BAL.Listings;
using Microsoft.AspNetCore.Identity.UI.Services;
using BAL.Audit;
using System.Data;
using System;
using System.Collections.Generic;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.SubscriptionsEdit.Controllers
{
    [Area("SubscriptionsEdit")]
    [Authorize]
    public class WorkingHoursController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly IUserService _userService;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;

        public WorkingHoursController(ListingDbContext listingContext, IUserService userService,
            IHistoryAudit audit, IListingManager listingManager)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.audit = audit;
            this.listingManager = listingManager;
        }

        // GET: SubscriptionsEdit/WorkingHours/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Shafi: Get UserGuid & IP Address
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string ownerGuid = user.Id;
            // End:

            if (id == null)
            {
                return NotFound();
            }

            var workingHours = await listingContext.WorkingHours
                .FirstOrDefaultAsync(m => m.ListingID == id);
            if (workingHours == null)
            {
                return NotFound();
            }

            // Shafi: Find listing
            var listing = await listingContext.Listing.Where(l => l.ListingID == workingHours.ListingID).FirstOrDefaultAsync();
            // End: 

            // Shafi: Verify record ownership
            var owner = await listingManager.WorkingOwnerAsync(workingHours.WorkingHoursID, listing.ListingID, ownerGuid);
            // End:
            if (owner == true)
            {
                return View(workingHours);
            }
            else
            {
                return NotFound();
            }
        }

        // GET: SubscriptionsEdit/WorkingHours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Shafi: Get UserGuid & IP Address
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string ownerGuid = user.Id;
            // End:

            if (id == null)
            {
                return NotFound();
            }

            var workingHours = await listingContext.WorkingHours.FindAsync(id);
            if (workingHours == null)
            {
                return NotFound();
            }

            // Shafi: Find listing
            var listing = await listingContext.Listing.Where(l => l.ListingID == workingHours.ListingID).FirstOrDefaultAsync();
            // End: 

            // Shafi: Verify record ownership
            var owner = await listingManager.WorkingOwnerAsync(id.Value, listing.ListingID, ownerGuid);
            // End:
            if (owner == true)
            {
                return View(workingHours);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: SubscriptionsEdit/WorkingHours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WorkingHoursID,ListingID,OwnerGuid,IPAddress,MondayFrom,MondayTo,TuesdayFrom,TuesdayTo,WednesdayFrom,WednesdayTo,ThursdayFrom,ThursdayTo,FridayFrom,FridayTo,SaturdayHoliday,SaturdayFrom,SaturdayTo,SundayHoliday,SundayFrom,SundayTo")] WorkingHours workingHours)
        {
            // Shafi: Get UserGuid & IP Address
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string ownerGuid = user.Id;
            // End:

            if (id != workingHours.WorkingHoursID)
            {
                return NotFound();
            }

            // Shafi: Find listing
            var listing = await listingContext.Listing.Where(l => l.ListingID == workingHours.ListingID).FirstOrDefaultAsync();
            // End: 

            // Shafi: Verify ownership record
            var owner = await listingManager.WorkingOwnerAsync(workingHours.WorkingHoursID, listing.ListingID, ownerGuid);
            // End:

            if (owner == true && listing != null)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        // Shafi: Create last updated
                        string ipAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
                        string userAgent = this.HttpContext.Request.Headers["User-Agent"];
                        string referUrl = this.HttpContext.Request.Headers["Referer"];
                        string visitedURL = this.HttpContext.Request.Headers["Host"];
                        string userGuid = user.Id;
                        string email = user.Email;
                        // Shafi: Time zone
                        string mobile = user.PhoneNumber;
                        DateTime timeZoneDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        string updatedDate = timeZoneDate.ToString("d-MM-yyyy");
                        string updatedTime = timeZoneDate.ToString("hh:mm:ss tt");
                        // End:

                        // Shafi: Save context
                        workingHours.IPAddress = ipAddress;
                        listingContext.Update(workingHours);
                        await listingContext.SaveChangesAsync();
                        // End:

                        string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/SubscriptionsEdit/WorkingHours/Details/5" + listing.ListingID;
                        string activity = "Updated Working Hours for " + listing.CompanyName + " with id " + listing.ListingID;

                        // Shafi: Get user in roles
                        IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                        string roleName = userInRoleName.FirstOrDefault();
                        // End:

                        await audit.CreateListingLastUpdatedAsync(listing.ListingID, userGuid, email, mobile, ipAddress, roleName, "Working Hours", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                        // End:

                        // Shafi: Show success message in redirected view
                        TempData["SuccessMessage"] = "Working Hours updated successfully.";
                        // End:

                        return Redirect("/SubscriptionsEdit/WorkingHours/Details/" + listing.ListingID);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!WorkingHoursExists(workingHours.WorkingHoursID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    return View(workingHours);
                }
            }
            else
            {
                return NotFound();
            }
        }

        private bool WorkingHoursExists(int id)
        {
            return listingContext.WorkingHours.Any(e => e.WorkingHoursID == id);
        }
    }
}
