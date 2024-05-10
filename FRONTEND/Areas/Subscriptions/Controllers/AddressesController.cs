using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BOL.LISTING;
using DAL.LISTING;
using DAL.SHARED;
using Microsoft.AspNetCore.Identity;
using BAL.Audit;
using BAL.Listings;
using Microsoft.AspNetCore.Http;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
    public class AddressesController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly SharedDbContext sharedManager;
        private readonly IUserService _userService;

        public AddressesController(ListingDbContext listingContext, IUserService userService, 
            SharedDbContext sharedManager)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.sharedManager = sharedManager;
        }

        // GET: Subscriptions/Addresses
        public async Task<IActionResult> Index()
        {
            return View(await listingContext.Address.ToListAsync());
        }

        // GET: Subscriptions/Addresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await listingContext.Address
                .FirstOrDefaultAsync(m => m.AddressID == id);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // GET: Subscriptions/Addresses/Create
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

            // Shafi: Show success modal popup
            ViewBag.CreateMessage = TempData["CreateMessage"];
            ViewBag.CreateError = TempData["CreateError"];
            // End:

            ViewData["Countries"] = new SelectList(sharedManager.Country, "CountryID", "Name");

            return View();
        }

        // POST: Subscriptions/Addresses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AddressID,ListingID,OwnerGuid,IPAddress,CountryID,StateID,City,AssemblyID,PincodeID,LocalityID,LocalAddress")] Address address)
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
            address.OwnerGuid = ownerGuid;
            address.IPAddress = remoteIpAddress;
            address.ListingID = HttpContext.Session.GetInt32("ListingID").Value;
            // End:

            if (ModelState.IsValid)
            {
                listingContext.Add(address);
                await listingContext.SaveChangesAsync();
                return RedirectToAction("Create", "Categories", "Subscriptions");
            }

            ViewData["Countries"] = new SelectList(sharedManager.Country, "CountryID", "Name");
            return View(address);
        }

        // GET: Subscriptions/Addresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await listingContext.Address.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }
            return View(address);
        }

        // POST: Subscriptions/Addresses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AddressID,ListingID,OwnerGuid,IPAddress,CountryID,StateID,City,AssemblyID,PincodeID,LocalityID,LocalAddress")] Address address)
        {
            if (id != address.AddressID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    listingContext.Update(address);
                    await listingContext.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }

        // GET: Subscriptions/Addresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await listingContext.Address
                .FirstOrDefaultAsync(m => m.AddressID == id);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // POST: Subscriptions/Addresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var address = await listingContext.Address.FindAsync(id);
            listingContext.Address.Remove(address);
            await listingContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AddressExists(int id)
        {
            return listingContext.Address.Any(e => e.AddressID == id);
        }
    }
}
