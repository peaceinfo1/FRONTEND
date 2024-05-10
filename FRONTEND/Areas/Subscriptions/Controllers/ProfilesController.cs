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
    public class ProfilesController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly IUserService _userService;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;

        public ProfilesController(ListingDbContext listingContext, IUserService userService, 
            IHistoryAudit audit, IListingManager listingManager)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.listingManager = listingManager;
            this.audit = audit;
        }

        // GET: Subscriptions/Profiles/Create
        public IActionResult Create()
        {
            // Shafi: Get listing id from session
            ViewBag.ListingID = HttpContext.Session.GetInt32("ListingID");
            // End:

            return View();
        }

        // POST: Subscriptions/Profiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProfileID,ListingID,OwnerGuid,IPAddress,ProfileDetails")] Profile profile)
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
            profile.OwnerGuid = ownerGuid;
            profile.IPAddress = remoteIpAddress;
            profile.ListingID = HttpContext.Session.GetInt32("ListingID").Value;
            // End:

            if (ModelState.IsValid)
            {
                listingContext.Add(profile);
                await listingContext.SaveChangesAsync();
                return RedirectToAction("Create", "PaymentModes", "Subscriptions");
            }
            return View(profile);
        }

        // GET: Subscriptions/Profiles/Edit/5
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

            var profile = await listingContext.Profile.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            // Shafi: Verify record ownership
            var owner = await listingManager.ProfileOwnerAsync(profile.ProfileID, profile.ListingID, userGuid);
            if (owner == true)
            {
                return View(profile);
            }
            else
            {
                return NotFound();
            }
            // End:

        }

        // POST: Subscriptions/Profiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProfileID,ListingID,OwnerGuid,IPAddress,ProfileDetails")] Profile profile)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string userGuid = user.Id;
            // End:

            if (id != profile.ProfileID)
            {
                return NotFound();
            }

            // Shafi: Verify ownership
            var owner = await listingManager.ProfileOwnerAsync(profile.ProfileID, profile.ListingID, userGuid);
            if (owner == true)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        listingContext.Update(profile);
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
                        string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/Subscriptions/Profiles/Edit/" + profile.ProfileID;
                        string activity = "Updated branch with id " + profile.ProfileID;

                        // Shafi: Get user in roles
                        IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                        string roleName = userInRoleName.FirstOrDefault();
                        // End:

                        await audit.CreateListingLastUpdatedAsync(profile.ProfileID, userGuid, email, mobile, ipAddress, roleName, "Branch", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                        // End:
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProfileExists(profile.ProfileID))
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

            return View(profile);
        }

        private bool ProfileExists(int id)
        {
            return listingContext.Profile.Any(e => e.ProfileID == id);
        }
    }
}
