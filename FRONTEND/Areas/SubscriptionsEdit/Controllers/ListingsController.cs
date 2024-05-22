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
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Http;
using DAL.SHARED;
using BAL.Listings;
using Microsoft.AspNetCore.Identity.UI.Services;
using BAL.Audit;
using System.Data;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.SubscriptionsEdit.Controllers
{
    [Area("SubscriptionsEdit")]
    [Authorize]
    public class ListingsController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly SharedDbContext sharedContext;
        private readonly IUserService _userService;
        private readonly IEmailSender emailSender;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;

        public ListingsController(ListingDbContext listingContext, IUserService userService, 
            SharedDbContext sharedContext, IEmailSender emailSender, IHistoryAudit audit, IListingManager listingManager)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.sharedContext = sharedContext;
            this.emailSender = emailSender;
            this.audit = audit;
            this.listingManager = listingManager;
        }

        // GET: SubscriptionsEdit/Listings/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            var distinctKeywords = listingContext.Keywords.Select(k => k.SeoKeyword).Distinct().ToList();

            // Create a SelectList with distinct keywords
            ViewData["Keywords"] = new SelectList(distinctKeywords);
            ViewData["NatureOfBusiness"] = new SelectList(sharedContext.NatureOfBusiness, "Name", "Name");

            ViewData["Turnover"] = new SelectList(sharedContext.Turnover, "Name", "Name");

            ViewData["Designations"] = new SelectList(sharedContext.Designation, "Name", "Name");


            if (id == null)
            {
                return NotFound();
            }

            var listing = await listingContext.Listing
                .FirstOrDefaultAsync(m => m.ListingID == id);
            if (listing == null)
            {
                return NotFound();
            }

            return View(listing);
        }        

        // GET: SubscriptionsEdit/Listings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string OwnerGuid = user.Id;
            // End:

            if (id == null)
            {
                return NotFound();
            }

            var listing = await listingContext.Listing.FindAsync(id);
            if (listing == null)
            {
                return NotFound();
            }

            // Shafi: Verify record ownership
            //if (await listingManager.CompanyOwnerAsync(id.Value, OwnerGuid) == true)
            //{
                // Begin: Get All Business Category
                var businessCategories = await listingContext.Listing.Select(i => i.BusinessCategory).Distinct().ToListAsync();

                var businessCategoryList = new List<SelectListItem>();

                foreach(var cat in businessCategories)
                {
                    var item = new SelectListItem
                    {
                        Value = cat,
                        Text = cat
                    };

                    businessCategoryList.Add(item);
                }

                ViewData["BusinessCategory"] = new SelectList(businessCategoryList, "Value", "Text");
                // End: Get All Business Category

                var distinctKeywords = listingContext.Keywords.Select(k => k.SeoKeyword).Distinct().ToList();

                // Create a SelectList with distinct keywords
                ViewData["Keywords"] = new SelectList(distinctKeywords);
                ViewData["NatureOfBusiness"] = new SelectList(sharedContext.NatureOfBusiness, "Name", "Name");

                ViewData["Turnover"] = new SelectList(sharedContext.Turnover, "Name", "Name");

                ViewData["Designations"] = new SelectList(sharedContext.Designation, "Name", "Name");

                return View(listing);
            //}
            //else
            //{
            //    return NotFound();
            //}
            // End:
        }

        // POST: SubscriptionsEdit/Listings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ListingID,OwnerGuid,IPAddress,CreatedDate,CreatedTime,CompanyName,YearOfEstablishment,GSTNumber,BusinessCategory,NumberOfEmployees,NatureOfBusiness,Turnover,ListingURL,Description")] Listing listing)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string UserGuid = user.Id;
            // End:

            if (id != listing.ListingID)
            {
                return NotFound();
            }

            var distinctKeywords = listingContext.Keywords.Select(k => k.SeoKeyword).Distinct().ToList();

            // Create a SelectList with distinct keywords
            ViewData["Keywords"] = new SelectList(distinctKeywords);
            ViewData["NatureOfBusiness"] = new SelectList(sharedContext.NatureOfBusiness, "Name", "Name");
            ViewData["Turnover"] = new SelectList(sharedContext.Turnover, "Name", "Name");
            ViewData["Designations"] = new SelectList(sharedContext.Designation, "Name", "Name");

            var owner = await listingManager.CompanyOwnerAsync(id, UserGuid);
            if (owner == true)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        listingContext.Update(listing);
                        await listingContext.SaveChangesAsync();

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
                        string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/Subscriptions/Listings/Edit/" + listing.ListingID;
                        string activity = "Updated listing details " + listing.CompanyName + " with id " + listing.ListingID;

                        // Shafi: Get user in roles
                        IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                        string roleName = userInRoleName.FirstOrDefault();
                        // End:

                        await audit.CreateListingLastUpdatedAsync(listing.ListingID, userGuid, email, mobile, ipAddress, roleName, "Company", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                        // End:
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ListingExists(listing.ListingID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    // Shafi: Show success message in redirected view
                    TempData["SuccessMessage"] = "Company details saved successfully.";
                    // End:

                    return Redirect("/SubscriptionsEdit/Listings/Details/" + id);
                }
            }
            else
            {
                return NotFound();
            }

            return View(listing);
        }

        private bool ListingExists(int id)
        {
            return listingContext.Listing.Any(e => e.ListingID == id);
        }
    }
}
