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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using DAL.SHARED;
using BAL.Listings;
using BAL.Audit;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
    [Authorize]
    public class BranchesController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly IUserService _userService;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;

        public BranchesController(ListingDbContext listingContext, IUserService userService, 
            IHistoryAudit audit, IListingManager listingManager)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.listingManager = listingManager;
            this.audit = audit;
        }

        // GET: Subscriptions/Branches/Create
        public IActionResult Create()
        {
            // Shafi: Get listing id from session
            ViewBag.ListingID = HttpContext.Session.GetInt32("ListingID");
            // End:

            return View();
        }

        // POST: Subscriptions/Branches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BranchID,ListingID,OwnerGuid,IPAddress,BranchName,ContactPerson,Email,Mobile,Telephone,BranchAddress")] Branches branches)
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
            branches.OwnerGuid = ownerGuid;
            branches.IPAddress = remoteIpAddress;
            branches.ListingID = HttpContext.Session.GetInt32("ListingID").Value;
            // End:

            if (ModelState.IsValid)
            {
                listingContext.Add(branches);
                await listingContext.SaveChangesAsync();
                return RedirectToAction("Create", "Addresses", "Subscriptions");
            }
            return View(branches);
        }

        // GET: Subscriptions/Branches/Edit/5
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

            var branches = await listingContext.Branches.FindAsync(id);
            if (branches == null)
            {
                return NotFound();
            }

            // Shafi: Verify record ownership
            var owner = await listingManager.BranchesOwnerAsync(branches.BranchID, branches.ListingID, userGuid);
            if (owner == true)
            {
                return View(branches);
            }
            else
            {
                return NotFound();
            }
            // End:
        }

        // POST: Subscriptions/Branches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BranchID,ListingID,OwnerGuid,IPAddress,BranchName,ContactPerson,Email,Mobile,Telephone,BranchAddress")] Branches branches)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string userGuid = user.Id;
            // End:

            if (id != branches.BranchID)
            {
                return NotFound();
            }

            // Shafi: Verify ownership
            var owner = await listingManager.BranchesOwnerAsync(branches.BranchID, branches.ListingID, userGuid);
            if (owner == true)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        listingContext.Update(branches);
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
                        string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/Subscriptions/Listings/Edit/" + branches.BranchID;
                        string activity = "Updated branch details " + branches.BranchName + " with id " + branches.BranchID;

                        // Shafi: Get user in roles
                        IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                        string roleName = userInRoleName.FirstOrDefault();
                        // End:

                        await audit.CreateListingLastUpdatedAsync(branches.BranchID, userGuid, email, mobile, ipAddress, roleName, "Branch", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                        // End:
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!BranchesExists(branches.BranchID))
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

            return View(branches);
        }

        private bool BranchesExists(int id)
        {
            return listingContext.Branches.Any(e => e.BranchID == id);
        }
    }
}
