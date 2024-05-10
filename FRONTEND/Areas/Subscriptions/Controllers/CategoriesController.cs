using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BOL.LISTING;
using DAL.LISTING;
using DAL.CATEGORIES;
using Microsoft.AspNetCore.Identity;
using BAL.Audit;
using BAL.Listings;
using Microsoft.AspNetCore.Http;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
    public class CategoriesController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly CategoriesDbContext categoryContext;
        private readonly IUserService _userService;
        private readonly IHistoryAudit audit;

        public CategoriesController(ListingDbContext listingContext, IUserService userService, IHistoryAudit audit, CategoriesDbContext categoryContext)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.audit = audit;
            this.categoryContext = categoryContext;
        }

        // GET: Subscriptions/Categories
        public async Task<IActionResult> Index()
        {
            return View(await listingContext.Categories.ToListAsync());
        }

        // GET: Subscriptions/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categories = await listingContext.Categories
                .FirstOrDefaultAsync(m => m.CategoryID == id);
            if (categories == null)
            {
                return NotFound();
            }

            return View(categories);
        }

        // GET: Subscriptions/Categories/Create
        public IActionResult Create()
        {
            // Shafi: Check if user created the listing recently
            if (HttpContext.Session.GetInt32("ListingID") == null)
            {
                return RedirectToAction("Index", "Listings", "Subscriptions");
            }
            // End:

            ViewData["FirstCategories"] = new SelectList(categoryContext.FirstCategory, "FirstCategoryID", "Name");

            // Shafi: Get listing id from session
            ViewBag.ListingID = HttpContext.Session.GetInt32("ListingID");
            // End:
            return View();
        }

        // POST: Subscriptions/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryID,ListingID,OwnerGuid,IPAddress,FirstCategoryID,SecondCategoryID,ThirdCategories,FourthCategories,FifthCategories,SixthCategories")] Categories categories)
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
            categories.OwnerGuid = ownerGuid;
            categories.IPAddress = remoteIpAddress;
            categories.ListingID = HttpContext.Session.GetInt32("ListingID").Value;
            // End:

            if (ModelState.IsValid)
            {
                listingContext.Add(categories);
                await listingContext.SaveChangesAsync();

                return RedirectToAction("Create", "Specialisations", "Subscriptions");
            }
            return View(categories);
        }

        // GET: Subscriptions/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categories = await listingContext.Categories.FindAsync(id);
            if (categories == null)
            {
                return NotFound();
            }
            return View(categories);
        }

        // POST: Subscriptions/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryID,ListingID,OwnerGuid,IPAddress,FirstCategoryID,SecondCategoryID,ThirdCategories,FourthCategories,FifthCategories,SixthCategories")] Categories categories)
        {
            if (id != categories.CategoryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    listingContext.Update(categories);
                    await listingContext.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            return View(categories);
        }

        // GET: Subscriptions/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categories = await listingContext.Categories
                .FirstOrDefaultAsync(m => m.CategoryID == id);
            if (categories == null)
            {
                return NotFound();
            }

            return View(categories);
        }

        // POST: Subscriptions/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categories = await listingContext.Categories.FindAsync(id);
            listingContext.Categories.Remove(categories);
            await listingContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriesExists(int id)
        {
            return listingContext.Categories.Any(e => e.CategoryID == id);
        }
    }
}
