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

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
    public class AssembliesAndCitiesController : Controller
    {
        private readonly ListingDbContext _context;
        private readonly SharedDbContext sharedContext;

        public AssembliesAndCitiesController(ListingDbContext context, SharedDbContext sharedContext)
        {
            _context = context;
            this.sharedContext = sharedContext;
        }

        // GET: Subscriptions/AssembliesAndCities
        public async Task<IActionResult> Index()
        {
            return View(await _context.AssembliesAndCities.ToListAsync());
        }

        // GET: Subscriptions/AssembliesAndCities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assembliesAndCities = await _context.AssembliesAndCities
                .FirstOrDefaultAsync(m => m.AssemblyAndCityID == id);
            if (assembliesAndCities == null)
            {
                return NotFound();
            }

            return View(assembliesAndCities);
        }

        // GET: Subscriptions/AssembliesAndCities/Create
        public IActionResult Create()
        {
            ViewData["Countries"] = new SelectList(sharedContext.Country, "CountryID", "Name");
            return View();
        }

        // POST: Subscriptions/AssembliesAndCities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AssemblyAndCityID,ListingID,OwnerGuid,IPAddress,CountryID,StateID,CityIDs,AssemblyIDs")] AssembliesAndCities assembliesAndCities)
        {
            ViewData["CountryID"] = new SelectList(sharedContext.Country, "CountryID", "Name");

            if (ModelState.IsValid)
            {
                _context.Add(assembliesAndCities);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(assembliesAndCities);
        }

        // GET: Subscriptions/AssembliesAndCities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assembliesAndCities = await _context.AssembliesAndCities.FindAsync(id);
            if (assembliesAndCities == null)
            {
                return NotFound();
            }
            return View(assembliesAndCities);
        }

        // POST: Subscriptions/AssembliesAndCities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AssemblyAndCityID,ListingID,OwnerGuid,IPAddress,CountryID,StateID,CityIDs,AssemblyIDs")] AssembliesAndCities assembliesAndCities)
        {
            if (id != assembliesAndCities.AssemblyAndCityID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assembliesAndCities);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssembliesAndCitiesExists(assembliesAndCities.AssemblyAndCityID))
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
            return View(assembliesAndCities);
        }

        // GET: Subscriptions/AssembliesAndCities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assembliesAndCities = await _context.AssembliesAndCities
                .FirstOrDefaultAsync(m => m.AssemblyAndCityID == id);
            if (assembliesAndCities == null)
            {
                return NotFound();
            }

            return View(assembliesAndCities);
        }

        // POST: Subscriptions/AssembliesAndCities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assembliesAndCities = await _context.AssembliesAndCities.FindAsync(id);
            _context.AssembliesAndCities.Remove(assembliesAndCities);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssembliesAndCitiesExists(int id)
        {
            return _context.AssembliesAndCities.Any(e => e.AssemblyAndCityID == id);
        }
    }
}
