using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BOL.SHARED;
using DAL.SHARED;
using Microsoft.AspNetCore.Authorization;

namespace FRONTEND.Areas.AjaxRequests.Controllers
{
    [Area("AjaxRequests")]
    public class AddressesController : Controller
    {
        private readonly SharedDbContext _context;

        public AddressesController(SharedDbContext context)
        {
            _context = context;
        }      
        

        // POST: Common/Stations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssembly([Bind("StationID,Name,CityID")] Location station)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Location.AnyAsync((System.Linq.Expressions.Expression<Func<Location, bool>>)(a => (bool)(a.Name.Contains((string)station.Name) && a.CityID == station.CityID))))
                {
                    TempData["CreateError"] = station.Name + " already exisits in assembly database.";
                    return base.Redirect("/Subscriptions/Addresses/Create");
                }

                _context.Add(station);
                await _context.SaveChangesAsync();

                TempData["CreateMessage"] = station.Name + " created successfully.";
                return Redirect("/Subscriptions/Addresses/Create");
            }

            TempData["CreateError"] = "Oops! Something went wrong, please try again.";
            return Redirect("/Subscriptions/Addresses/Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePincode([Bind("PincodeID,PincodeNumber,StationID")] Pincode pincode)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Pincode.AnyAsync(p => p.PincodeNumber == pincode.PincodeNumber && p.LocationId == pincode.LocationId))
                {
                    TempData["CreateError"] = pincode.PincodeNumber + " already exisits in pincode database.";
                    return Redirect("/Subscriptions/Addresses/Create");
                }

                _context.Add(pincode);
                await _context.SaveChangesAsync();

                TempData["CreateMessage"] = pincode.PincodeNumber + " created successfully.";
                return Redirect("/Subscriptions/Addresses/Create");
            }

            TempData["CreateError"] = "Oops! Something went wrong while creating pincode, please try again.";
            return Redirect("/Subscriptions/Addresses/Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocality([Bind("LocalityID,LocalityName,StationID,PincodeID")] Area locality)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Area.AnyAsync((System.Linq.Expressions.Expression<Func<Area, bool>>)(p => (bool)(p.Name.Contains((string)locality.Name) && p.PincodeID == locality.PincodeID && locality.LocationId == locality.LocationId))))
                {
                    TempData["CreateError"] = locality.Name + " already exisits in locality database.";
                    return base.Redirect("/Subscriptions/Addresses/Create");
                }

                _context.Add(locality);
                await _context.SaveChangesAsync();

                TempData["CreateMessage"] = "Locality " + locality.Name + " created successfully.";
                return Redirect("/Subscriptions/Addresses/Create");
            }

            TempData["CreateError"] = "Oops! Something went wrong while creating locality, please try again.";
            return Redirect("/Subscriptions/Addresses/Create");
        }
    }
}
