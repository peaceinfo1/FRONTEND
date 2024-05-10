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
    public class AddressesController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly SharedDbContext sharedContext;
        private readonly IUserService _userService;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;

        public AddressesController(ListingDbContext listingContext, IUserService userService,
            SharedDbContext sharedContext, IHistoryAudit audit, IListingManager listingManager)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.sharedContext = sharedContext;
            this.audit = audit;
            this.listingManager = listingManager;
        }

        // GET: SubscriptionsEdit/Addresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await listingContext.Address
                .FirstOrDefaultAsync(m => m.ListingID == id);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // GET: SubscriptionsEdit/Addresses/Edit/5
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

            var address = await listingContext.Address.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            var owner = await listingManager.AddressOwnerAsync(id.Value, address.ListingID, OwnerGuid);
            if(owner == true)
            {
                ViewData["Countries"] = new SelectList(sharedContext.Country, "CountryID", "Name");
                return View(address);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: SubscriptionsEdit/Addresses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AddressID,ListingID,OwnerGuid,IPAddress,CountryID,StateID,City,AssemblyID,PincodeID,LocalityID,LocalAddress")] Address address)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string OwnerGuid = user.Id;
            // End:

            if (id != address.AddressID)
            {
                return NotFound();
            }

            // Shafi: Verify record ownership
            var owner = await listingManager.AddressOwnerAsync(id, address.ListingID, OwnerGuid);
            if (owner == true)
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

                        // Shafi: Save changes
                        address.IPAddress = ipAddress;
                        listingContext.Update(address);
                        await listingContext.SaveChangesAsync();
                        // End:

                        // Shafi: Get Listing
                        var listing = await listingContext.Listing.Where(l => l.ListingID == address.ListingID).FirstOrDefaultAsync();
                        string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/SubscriptionsEdit/Communications/Edit/" + address.ListingID;
                        string activity = "Updated listing details " + listing.CompanyName + " with id " + listing.ListingID;
                        // End:

                        // Shafi: Get user in roles
                        IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                        string roleName = userInRoleName.FirstOrDefault();
                        // End:

                        await audit.CreateListingLastUpdatedAsync(listing.ListingID, userGuid, email, mobile, ipAddress, roleName, "Address", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                        // End:

                        // Shafi: Show success message in redirected view
                        TempData["SuccessMessage"] = "Address details saved successfully.";
                        // End:

                        // Shafi: Find listing ID
                        //var listingId = await listingContext.Address.Where(i => i.AddressID == id).Select(i => i.ListingID).FirstOrDefaultAsync();
                        // End:

                        return Redirect("/SubscriptionsEdit/Addresses/Details/" + address.ListingID);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!AddressExists(address.AddressID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            // End:
            ViewData["Countries"] = new SelectList(sharedContext.Country, "CountryID", "Name");
            return View(address);
        }

        private bool AddressExists(int id)
        {
            return listingContext.Address.Any(e => e.AddressID == id);
        }
    }
}
