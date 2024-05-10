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

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
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
            this.listingManager = listingManager;
            this.audit = audit;
        }

        // GET: Subscriptions/WorkingHours/Create
        public IActionResult Create()
        {
            // Shafi: Check if user created the listing recently
            if (HttpContext.Session.GetInt32("ListingID") == null)
            {
                return RedirectToAction("Index", "Listings", "Subscriptions");
            }
            // End:

            // Shafi: Get listing id from session
            ViewBag.ListingID = HttpContext.Session.GetInt32("ListingID");
            // End:

            return View();
        }

        // POST: Subscriptions/WorkingHours/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WorkingHoursID,ListingID,OwnerGuid,IPAddress,MondayFrom,MondayTo,TuesdayFrom,TuesdayTo,WednesdayFrom,WednesdayTo,ThursdayFrom,ThursdayTo,FridayFrom,FridayTo,SaturdayHoliday,SaturdayFrom,SaturdayTo,SundayHoliday,SundayFrom,SundayTo")] WorkingHours workingHours)
        {
            // Shafi: Get UserGuid & IP Address
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string ownerGuid = user.Id;
            // End:
            // Shafi: Get listing id from session
            ViewBag.ListingID = HttpContext.Session.GetInt32("ListingID");
            // End:
            // Shafi: Assign values in background
            workingHours.OwnerGuid = ownerGuid;
            workingHours.IPAddress = remoteIpAddress;
            workingHours.ListingID = HttpContext.Session.GetInt32("ListingID").Value;
            // End:

            if (ModelState.IsValid)
            {
                listingContext.Add(workingHours);
                await listingContext.SaveChangesAsync();
                return RedirectToAction("Create", "PaymentModes", "Subscriptions");
            }
            return View(workingHours);
        }

        // GET: Subscriptions/WorkingHours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string userGuid = user.Id;
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

            // Shafi: Verify record ownership
            var owner = await listingManager.WorkingOwnerAsync(workingHours.WorkingHoursID, workingHours.ListingID, userGuid);
            if (owner == true)
            {
                return View(workingHours);
            }
            else
            {
                return NotFound();
            }
            // End:
        }

        // POST: Subscriptions/WorkingHours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WorkingHoursID,ListingID,OwnerGuid,IPAddress,MondayFrom,MondayTo,TuesdayFrom,TuesdayTo,WednesdayFrom,WednesdayTo,ThursdayFrom,ThursdayTo,FridayFrom,FridayTo,SaturdayHoliday,SaturdayFrom,SaturdayTo,SundayHoliday,SundayFrom,SundayTo")] WorkingHours workingHours)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string userGuid = user.Id;
            // End:

            if (id != workingHours.WorkingHoursID)
            {
                return NotFound();
            }

            // Shafi: Verify ownership
            var owner = await listingManager.WorkingOwnerAsync(workingHours.WorkingHoursID, workingHours.ListingID, userGuid);
            if (owner == true)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        listingContext.Update(workingHours);
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
                        string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/Subscriptions/WorkingHours/Edit/" + workingHours.WorkingHoursID;
                        string activity = "Updated working hours with id " + workingHours.WorkingHoursID;

                        // Shafi: Get user in roles
                        IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                        string roleName = userInRoleName.FirstOrDefault();
                        // End:

                        await audit.CreateListingLastUpdatedAsync(workingHours.WorkingHoursID, userGuid, email, mobile, ipAddress, roleName, "Working Hours", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                        // End:
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

                    return RedirectToAction("Index", "Listings", "Subscriptions");

                }
            }
            else
            {
                return NotFound();
            }

            return View(workingHours);
        }

        private bool WorkingHoursExists(int id)
        {
            return listingContext.WorkingHours.Any(e => e.WorkingHoursID == id);
        }
    }
}
