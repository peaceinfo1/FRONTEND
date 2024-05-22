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
using DAL.SHARED;
using BAL.Listings;
using BAL.Audit;
using System.Data;
using BOL.VIEWMODELS;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
    [Authorize]
    public class ListingsController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly SharedDbContext sharedManager;
        private readonly IUserService _userService;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;

        public ListingsController(ListingDbContext listingContext, IUserService userService, 
            SharedDbContext sharedManager, IHistoryAudit audit, IListingManager listingManager)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.sharedManager = sharedManager;
            this.audit = audit;
            this.listingManager = listingManager;
        }

        // GET: Subscriptions/Listings
        public async Task<IActionResult> Index()
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string OwnerGuid = user.Id;
            // End:

            // Shafi: Display popup after creating listing
            // Taken from PaymentModes Create Controller
            ViewBag.ListingCreatedSuccessfully = TempData["ListingCreatedSuccessfully"];
            // End:

            var allListingForUsers = await listingContext.Listing.Where(i => i.OwnerGuid == OwnerGuid).ToListAsync();

            //IList<FreeListingViewModel> freeListingViewModelList = new List<FreeListingViewModel>();

            //var result = from list in listingContext.Listing
            //             .Where(l => l.OwnerGuid == OwnerGuid)
            //             join comm in listingContext.Communication
            //             on list.OwnerGuid equals comm.OwnerGuid

            //             join add in listingContext.Address
            //             on list.OwnerGuid equals add.OwnerGuid

            //             join cat in listingContext.Categories
            //             on list.OwnerGuid equals cat.OwnerGuid

            //             join spec in listingContext.Specialisation
            //             on list.OwnerGuid equals spec.OwnerGuid

            //             join work in listingContext.WorkingHours
            //             on list.OwnerGuid equals work.OwnerGuid

            //             join pay in listingContext.PaymentMode
            //             on list.OwnerGuid equals pay.OwnerGuid

            //             select new FreeListingViewModel
            //             {
            //                 Listing = list,
            //                 Address = add,
            //                 Categories = cat,
            //                 Specialisation = spec,
            //                 WorkingHours = work,
            //                 PaymentMode = pay
            //             };

            //foreach(var item in result)
            //{
            //    FreeListingViewModel freeListingViewModel = new FreeListingViewModel
            //    {
            //        Listing = item.Listing,
            //        Communication = item.Communication,
            //        Address = item.Address,
            //        Categories = item.Categories,
            //        Specialisation = item.Specialisation,
            //        WorkingHours = item.WorkingHours,
            //        PaymentMode = item.PaymentMode
            //    };

            //    freeListingViewModelList.Add(freeListingViewModel);
            //}

            var result = from list in listingContext.Listing
                         .Where(i => i.OwnerGuid == OwnerGuid)
                         join comm in listingContext.Communication
                         on list.ListingID equals comm.ListingID

                         join add in listingContext.Address
                         on list.ListingID equals add.ListingID

                         join cat in listingContext.Categories
                               on list.ListingID equals cat.ListingID

                         join spec in listingContext.Specialisation
                               on list.ListingID equals spec.ListingID

                         join work in listingContext.WorkingHours
                               on list.ListingID equals work.ListingID

                         join pay in listingContext.PaymentMode
                               on list.ListingID equals pay.ListingID

                         select new FreeListingViewModel
                         {
                             Listing = list,
                             Communication = comm,
                             //Address = add,
                             //Categories = cat,
                             Specialisation = spec,
                             //WorkingHours = work,
                             PaymentMode = pay
                         };

            return View(await result.OrderByDescending(l => l.Listing.CreatedDate).ToListAsync());

            //return View(allListingForUsers.ToList());
        }

        // GET: Subscriptions/Listings/Create
        [HttpGet]
        public async Task<IActionResult> Create(DateTime visitDate, DateTime visitTime)
        {
            // Shafi: Get current user  
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            // End:

            if (User.IsInRole("Listing Manager") || User.IsInRole("Listings") || User.IsInRole("Super Administrator"))
            {
                var distinctKeywords = listingContext.Keywords.Select(k => k.SeoKeyword).Distinct().ToList();

                // Create a SelectList with distinct keywords
                ViewData["Keywords"] = new SelectList(distinctKeywords);

                //ViewData["Keywords"] = new SelectList(listingContext.Keywords, "SeoKeyword", "SeoKeyword");
                ViewData["NatureOfBusiness"] = new SelectList(sharedManager.NatureOfBusiness, "Name", "Name");
                ViewData["Turnover"] = new SelectList(sharedManager.Turnover, "Name", "Name");
                ViewData["Designations"] = new SelectList(sharedManager.Designation, "Name", "Name");
                return View();
            }
            else if (User.IsInRole("Listing Manager") == false || User.IsInRole("Listings") == false || User.IsInRole("Super Administrator") == false)
            {
                if (await listingManager.CheckIfUserHas5Listings(user.Id) != true)
                {
                    var distinctKeywords = listingContext.Keywords.Select(k => k.SeoKeyword).Distinct().ToList();

                    // Create a SelectList with distinct keywords
                    ViewData["Keywords"] = new SelectList(distinctKeywords);

                    //ViewData["Keywords"] = new SelectList(listingContext.Keywords, "SeoKeyword", "SeoKeyword");
                    ViewData["NatureOfBusiness"] = new SelectList(sharedManager.NatureOfBusiness, "Name", "Name");
                    ViewData["Turnover"] = new SelectList(sharedManager.Turnover, "Name", "Name");
                    ViewData["Designations"] = new SelectList(sharedManager.Designation, "Name", "Name");

                    return View();
                }
                else
                {

                    TempData["Message"] = "Free users can create only 3 listings.";
                    TempData["Description"] = "To create more listing please subscribe to paid membership plans.";
                    TempData["RefererUrl"] = Request.Headers["Referer"].ToString();

                    return RedirectToAction("FreeUsersCanCreate5Listings", "ErrorsAndExceptions", "Subscriptions");
                }
            }


            if (await listingManager.CheckIfUserHas5Listings(user.Id) != true)
            {
                var distinctKeywords = listingContext.Keywords.Select(k => k.SeoKeyword).Distinct().ToList();

                // Create a SelectList with distinct keywords
                ViewData["Keywords"] = new SelectList(distinctKeywords);
                ViewData["NatureOfBusiness"] = new SelectList(sharedManager.NatureOfBusiness, "Name", "Name");
                ViewData["Turnover"] = new SelectList(sharedManager.Turnover, "Name", "Name");
                ViewData["Designations"] = new SelectList(sharedManager.Designation, "Name", "Name");

                return View();
            }
            else
            {

                TempData["Message"] = "Free users can create only 3 listings.";
                TempData["Description"] = "To create more listing please subscribe to paid membership plans.";
                TempData["RefererUrl"] = Request.Headers["Referer"].ToString();

                return RedirectToAction("FreeUsersCanCreate5Listings", "ErrorsAndExceptions", "Subscriptions");
            }
        }

        // POST: Subscriptions/Listings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ListingID,OwnerGuid,IPAddress,CreatedDate,CreatedTime,CompanyName,YearOfEstablishment,GSTNumber,BusinessCategory,NumberOfEmployees,NatureOfBusiness,Turnover,ListingURL,Description, Approved")] Listing listing)
        {
            // Shafi: Dropdown

            var distinctKeywords = listingContext.Keywords.Select(k => k.SeoKeyword).Distinct().ToList();

            // Create a SelectList with distinct keywords
            ViewData["Keywords"] = new SelectList(distinctKeywords);

            //ViewData["Keywords"] = new SelectList(listingContext.Keywords, "SeoKeyword", "SeoKeyword");
            ViewData["NatureOfBusiness"] = new SelectList(sharedManager.NatureOfBusiness, "Name", "Name");
            ViewData["Turnover"] = new SelectList(sharedManager.Turnover, "Name", "Name");
            //ViewData["Designations"] = new SelectList(sharedManager.Designation, "Name", "Name");
            // End:

            // Shafi: Get UserGuid & IP Address
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string ownerGuid = user.Id;
            // End:

            // Shafi: Assign values in background
            listing.OwnerGuid = ownerGuid;
            listing.IPAddress = remoteIpAddress;
            // End:

            // Shafi: Assign Time Zone to CreatedDate & Created Time
            string mobile = user.PhoneNumber;
            DateTime timeZoneDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            listing.CreatedDate = timeZoneDate;
            listing.CreatedTime = timeZoneDate;
            // End:

            if (User.IsInRole("Listing Manager") || User.IsInRole("Listings") || User.IsInRole("Super Administrator"))
            {
                listing.Status = Listing.Approved;

                if (ModelState.IsValid)
                {
                    listingContext.Add(listing);
                    await listingContext.SaveChangesAsync();

                    // Shafi: Save listing id in session for form wizard
                    HttpContext.Session.SetInt32("ListingID", listing.ListingID);
                    // End:

                    // Shafi: Get listing id from session for form wizard
                    HttpContext.Session.GetInt32("ListingID");
                    // End:

                    return RedirectToAction("Create", "Communications", "Subscriptions");
                }
                return View(listing);
                //return RedirectToAction("Create", "Communications", "Subscriptions");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    listingContext.Add(listing);
                    await listingContext.SaveChangesAsync();

                    // Shafi: Save listing id in session for form wizard
                    HttpContext.Session.SetInt32("ListingID", listing.ListingID);
                    // End:

                    // Shafi: Get listing id from session for form wizard
                    HttpContext.Session.GetInt32("ListingID");
                    // End:

                    return RedirectToAction("Create", "Communications", "Subscriptions");
                }
                return View(listing);
            }
        }

        // GET: Subscriptions/Listings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string OwnerGuid = user.Id;
            // End:

            // Shafi: Browser header
            string ipAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string userAgent = this.HttpContext.Request.Headers["User-Agent"];
            string referUrl = this.HttpContext.Request.Headers["Referer"];
            string visitedURL = this.HttpContext.Request.Headers["Host"];
            // End: 

            // Shafi: Dropdown
            ViewData["NatureOfBusiness"] = new SelectList(sharedManager.NatureOfBusiness, "Name", "Name");
            ViewData["Turnover"] = new SelectList(sharedManager.Turnover, "Name", "Name");
            ViewData["Designations"] = new SelectList(sharedManager.Designation, "Name", "Name");
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
            if (await listingManager.CompanyOwnerAsync(id.Value, OwnerGuid) == true)
            {
                return View(listing);
            }
            else
            {
                return NotFound();
            }
            // End:
        }

        // POST: Subscriptions/Listings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ListingID,OwnerGuid,IPAddress,CreatedDate,CreatedTime,Name,Gender,CompanyName,YearOfEstablishment,NumberOfEmployees,Designation,NatureOfBusiness,Turnover,ListingURL")] Listing listing)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string UserGuid = user.Id;
            // End:

            // Shafi: Dropdown
            ViewData["NatureOfBusiness"] = new SelectList(sharedManager.NatureOfBusiness, "Name", "Name");
            ViewData["Turnover"] = new SelectList(sharedManager.Turnover, "Name", "Name");
            ViewData["Designations"] = new SelectList(sharedManager.Designation, "Name", "Name");
            // End:

            if (id != listing.ListingID)
            {
                return NotFound();
            }

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

                    return RedirectToAction(nameof(Index));
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