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
            this.listingManager = listingManager;
            this.audit = audit;
        }

        // GET: Subscriptions/Specialisations/Create
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

        // POST: Subscriptions/Specialisations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpecialisationID,ListingID,OwnerGuid,IPAddress,Banks,BeautyParlors,Bungalow,CallCenter,Church,Company,ComputerInstitute,Dispensary,ExhibitionStall,Factory,Farmhouse,Gurudwara,Gym,HealthClub,Home,Hospital,Hotel,Laboratory,Mandir,Mosque,Office,Plazas,ResidentialSociety,Resorts,Restaurants,Salons,Shop,ShoppingMall,Showroom,Warehouse")] Specialisation specialisation)
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
            specialisation.OwnerGuid = ownerGuid;
            specialisation.IPAddress = remoteIpAddress;
            specialisation.ListingID = HttpContext.Session.GetInt32("ListingID").Value;
            // End:

            if (ModelState.IsValid)
            {
                listingContext.Add(specialisation);
                await listingContext.SaveChangesAsync();
                return RedirectToAction("Create", "WorkingHours", "Subscriptions");
            }
            return View(specialisation);
        }

        // GET: Subscriptions/Specialisations/Edit/5
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

            var specialisation = await listingContext.Specialisation.FindAsync(id);
            if (specialisation == null)
            {
                return NotFound();
            }

            // Shafi: Verify record ownership
            var owner = await listingManager.SpecialisationOwnerAsync(specialisation.SpecialisationID, specialisation.ListingID, userGuid);
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

        // POST: Subscriptions/Specialisations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SpecialisationID,ListingID,OwnerGuid,IPAddress,Banks,BeautyParlors,Bungalow,CallCenter,Church,Company,ComputerInstitute,Dispensary,ExhibitionStall,Factory,Farmhouse,Gurudwara,Gym,HealthClub,Home,Hospital,Hotel,Laboratory,Mandir,Mosque,Office,Plazas,ResidentialSociety,Resorts,Restaurants,Salons,Shop,ShoppingMall,Showroom,Warehouse")] Specialisation specialisation)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string userGuid = user.Id;
            // End:

            if (id != specialisation.SpecialisationID)
            {
                return NotFound();
            }

            // Shafi: Verify ownership
            var owner = await listingManager.SpecialisationOwnerAsync(specialisation.SpecialisationID, specialisation.ListingID, userGuid);
            if (owner == true)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        listingContext.Update(specialisation);
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
                        string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/Subscriptions/Specialisations/Edit/" + specialisation.SpecialisationID;
                        string activity = "Updated specialisation where id is " + specialisation.SpecialisationID;

                        // Shafi: Get user in roles
                        IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                        string roleName = userInRoleName.FirstOrDefault();
                        // End:

                        await audit.CreateListingLastUpdatedAsync(specialisation.SpecialisationID, userGuid, email, mobile, ipAddress, roleName, "Specialisation", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                        // End:
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

                    return RedirectToAction("Index", "Listings", "Subscriptions");

                }
            }
            else
            {
                return NotFound();
            }

            return View(specialisation);
        }

        private bool SpecialisationExists(int id)
        {
            return listingContext.Specialisation.Any(e => e.SpecialisationID == id);
        }
    }
}
