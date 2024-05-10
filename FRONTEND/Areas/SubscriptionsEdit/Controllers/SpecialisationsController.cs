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
    public class SpecialisationsController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly IUserService _userService;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;

        public SpecialisationsController(ListingDbContext listingContext, IUserService userService,
            IHistoryAudit audit, IListingManager listingManager)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.audit = audit;
            this.listingManager = listingManager;
        }

        // GET: SubscriptionsEdit/Specialisations/Details/5
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

            var specialisation = await listingContext.Specialisation
                .FirstOrDefaultAsync(i => i.ListingID == id);
            if (specialisation == null)
            {
                return NotFound();
            }

            // Shafi: Find listing
            var listing = await listingContext.Listing.Where(l => l.ListingID == specialisation.ListingID).FirstOrDefaultAsync();
            // End: 

            // Shafi: Verify ownership record
            var owner = await listingManager.SpecialisationOwnerAsync(specialisation.SpecialisationID, listing.ListingID, ownerGuid);

            if (owner == true)
            {
                return View(specialisation);
            }
            else
            {
                return NotFound();
            }
            // End:
        }

        // GET: SubscriptionsEdit/Specialisations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Shafi: Get UserGuid & IP Address
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string ownerGuid = user.Id;
            // End:

            if (id == null)
            {
                return NotFound();
            }

            var specialisation = await listingContext.Specialisation.FindAsync(id);

            if (specialisation == null)
            {
                return NotFound();
            }

            // Shafi: Find listing
            var listing = await listingContext.Listing.Where(l => l.ListingID == specialisation.ListingID).FirstOrDefaultAsync();
            // End: 

            // Shafi: Verify ownership record
            var owner = await listingManager.SpecialisationOwnerAsync(specialisation.SpecialisationID, listing.ListingID, ownerGuid);

            if (owner == true)
            {
                return View(specialisation);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: SubscriptionsEdit/Specialisations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SpecialisationID,ListingID,OwnerGuid,IPAddress,AcceptTenderWork,Banks,BeautyParlors,Bungalow,CallCenter,Church,Company,ComputerInstitute,Dispensary,ExhibitionStall,Factory,Farmhouse,Gurudwara,Gym,HealthClub,Home,Hospital,Hotel,Laboratory,Mandir,Mosque,Office,Plazas,ResidentialSociety,Resorts,Restaurants,Salons,Shop,ShoppingMall,Showroom,Warehouse")] Specialisation specialisation)
        {
            // Shafi: Get UserGuid & IP Address
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string ownerGuid = user.Id;
            // End:

            if (id != specialisation.SpecialisationID)
            {
                return NotFound();
            }

            // Shafi: Find specialisation
            //var specialisationDummy = await listingContext.Specialisation.Where(l => l.SpecialisationID == id).FirstOrDefaultAsync();
            // End:

            // Shafi: Find listing  
            var listing = await listingContext.Listing.Where(l => l.ListingID == specialisation.ListingID).FirstOrDefaultAsync();
            // End: 

            // Shafi: Check if listing listing is null or not
            if (listing != null)
            {
                // Shafi: Verify ownership record
                var owner = await listingManager.SpecialisationOwnerAsync(id, listing.ListingID, ownerGuid);

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

                            // Shafi: Save context
                            specialisation.IPAddress = ipAddress;
                            listingContext.Update(specialisation);
                            await listingContext.SaveChangesAsync();
                            // End:

                            string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/SubscriptionsEdit/Specialisations/Details/5" + listing.ListingID;
                            string activity = "Updated specialisations for " + listing.CompanyName + " with id " + listing.ListingID;

                            // Shafi: Get user in roles
                            IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                            string roleName = userInRoleName.FirstOrDefault();
                            // End:

                            await audit.CreateListingLastUpdatedAsync(listing.ListingID, userGuid, email, mobile, ipAddress, roleName, "Specialisations", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                            // End:

                            // Shafi: Show success message in redirected view
                            TempData["SuccessMessage"] = "Specialisations updated successfully.";
                            // End:

                            return Redirect("/SubscriptionsEdit/Specialisations/Details/" + listing.ListingID);

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!SpecialisationExists(specialisation.SpecialisationID))
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
                return NotFound();
            }
            // End:
            else
            {
                return NotFound();
            }
        }

        private bool SpecialisationExists(int id)
        {
            return listingContext.Specialisation.Any(e => e.SpecialisationID == id);
        }
    }
}
