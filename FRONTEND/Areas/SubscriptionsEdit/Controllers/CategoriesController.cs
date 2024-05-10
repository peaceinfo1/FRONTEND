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
using BAL.Listings;
using BAL.Audit;
using System.Data;
using DAL.CATEGORIES;
using BAL.Services.Contracts;
using DAL.Models;

namespace FRONTEND.Areas.SubscriptionsEdit.Controllers
{
    [Area("SubscriptionsEdit")]
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly IUserService _userService;
        private readonly CategoriesDbContext categoryContext;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;

        public CategoriesController(ListingDbContext listingContext, IUserService userService,
            IHistoryAudit audit, IListingManager listingManager, CategoriesDbContext categoryContext)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.audit = audit;
            this.listingManager = listingManager;
            this.categoryContext = categoryContext;
        }

        // GET: SubscriptionsEdit/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categories = await listingContext.Categories.Where(i => i.ListingID == id).FirstOrDefaultAsync();
            if (categories == null)
            {
                return NotFound();
            }

            return View(categories);
        }

        // GET: SubscriptionsEdit/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Shafi: Get UserGuid & IP Address
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string ownerGuid = user.Id;
            // End:

            ViewData["FirstCategories"] = new SelectList(categoryContext.FirstCategory, "FirstCategoryID", "Name");

            if (id == null)
            {
                return NotFound();
            }

            var categories = await listingContext.Categories.FindAsync(id);
            if (categories == null)
            {
                return NotFound();
            }

            // Shafi: Find listing
            var listing = await listingContext.Listing.Where(l => l.ListingID == categories.ListingID).FirstOrDefaultAsync();
            // End: 

            // Shafi: Verify ownership record
            var owner = await listingManager.CategoryOwnerAsync(id.Value, listing.ListingID, ownerGuid);
            if (owner == true)
            {
                return View(categories);
            }
            else
            {
                return NotFound();
            }
            // End:
        }

        // POST: SubscriptionsEdit/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryID,ListingID,OwnerGuid,IPAddress,FirstCategoryID,SecondCategoryID,ThirdCategories,FourthCategories,FifthCategories,SixthCategories")] Categories categories)
        {
            // Shafi: Get UserGuid & IP Address
            ApplicationUser user = await _userService.GetUserByUserName(User.Identity.Name);
            string remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string ownerGuid = user.Id;
            // End:

            // Shafi: Assign values in background
            categories.OwnerGuid = ownerGuid;
            categories.IPAddress = remoteIpAddress;
            // End:

            if (id != categories.CategoryID)
            {
                return NotFound();
            }

            // Shafi: Find listing
            var listing = await listingContext.Listing.Where(l => l.ListingID == categories.ListingID).FirstOrDefaultAsync();
            // End: 

            // Shafi: Verify ownership record
            var owner = await listingManager.CategoryOwnerAsync(id, listing.ListingID, ownerGuid);
            if(owner == true)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        listingContext.Update(categories);
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
                        string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/SubscriptionsEdit/Categories/Details/5" + listing.ListingID;
                        string activity = "Updated listing categories for " + listing.CompanyName + " with id " + listing.ListingID;

                        // Shafi: Get user in roles
                        IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                        string roleName = userInRoleName.FirstOrDefault();
                        // End:

                        await audit.CreateListingLastUpdatedAsync(listing.ListingID, userGuid, email, mobile, ipAddress, roleName, "Categories", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                        // End:
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CategoriesExists(categories.CategoryID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    // Shafi: Show success message in redirected view
                    TempData["SuccessMessage"] = "Categories updated successfully.";
                    // End:

                    return Redirect("/SubscriptionsEdit/Categories/Details/" + listing.ListingID);
                }
            }
            
            return View(categories);
        }

        private bool CategoriesExists(int id)
        {
            return listingContext.Categories.Any(e => e.CategoryID == id);
        }
    }
}
