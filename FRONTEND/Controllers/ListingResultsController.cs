using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BOL.LISTING;
using BOL.AUDITTRAIL;
using BOL.BILLING;
using BOL.CATEGORIES;
using BOL.PLAN;
using BOL.SHARED;
using BOL.VIEWMODELS;
using DAL.LISTING;
using DAL.AUDIT;
using DAL.BILLING;
using DAL.CATEGORIES;
using DAL.SHARED;
using Microsoft.EntityFrameworkCore;
using IDENTITY.Data;
using BAL.Audit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BAL.Listings;
using System;

namespace FRONTEND.Controllers
{
    public class ListingResultsController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly AuditDbContext auditContext;
        private readonly BillingDbContext billingContext;
        private readonly CategoriesDbContext categoriesContext;
        private readonly SharedDbContext sharedContext;
        private readonly ApplicationDbContext applicationContext;
        private readonly IHistoryAudit historyAudit;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IListingManager listingManager;

        public ListingResultsController(ListingDbContext listingContext, AuditDbContext auditContext, BillingDbContext billingContext, CategoriesDbContext categoriesContext, SharedDbContext sharedContext, ApplicationDbContext applicationContext, IHistoryAudit historyAudit, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IListingManager listingManager)
        {
            this.listingContext = listingContext;
            this.auditContext = auditContext;
            this.billingContext = billingContext;
            this.categoriesContext = categoriesContext;
            this.sharedContext = sharedContext;
            this.applicationContext = applicationContext;
            this.historyAudit = historyAudit;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.listingManager = listingManager;
        }

        [HttpGet]
        [Route("/ListingResults/{url}/{level}")]
        public async Task<IActionResult> Index(string url, string level)
        {
            if (level == "fc")
            {
                int id = await categoriesContext.FirstCategory.Where(c => c.URL == url).Select(c => c.FirstCategoryID).FirstOrDefaultAsync();

                var result = from list in listingContext.Listing
                             join comm in listingContext.Communication
                             on list.ListingID equals comm.ListingID

                             join add in listingContext.Address
                             on list.ListingID equals add.ListingID

                             join cat in listingContext.Categories
                                   .Where(c => c.FirstCategoryID == id)
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
                                 Address = add,
                                 Categories = cat,
                                 Specialisation = spec,
                                 WorkingHours = work,
                                 PaymentMode = pay
                             };

                return View(await result.ToListAsync());
            }
            else if (level == "sc")
            {
                int id = await categoriesContext.SecondCategory.Where(c => c.URL == url).Select(c => c.SecondCategoryID).FirstOrDefaultAsync();

                var result = from list in listingContext.Listing
                             join comm in listingContext.Communication
                             on list.ListingID equals comm.ListingID

                             join add in listingContext.Address
                             on list.ListingID equals add.ListingID

                             join cat in listingContext.Categories
                                   .Where(c => c.SecondCategoryID == id)
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
                                 Address = add,
                                 Categories = cat,
                                 Specialisation = spec,
                                 WorkingHours = work,
                                 PaymentMode = pay
                             };

                return View(await result.ToListAsync());
            }
            else if (level == "tc")
            {
                int id = await categoriesContext.ThirdCategory.Where(c => c.URL == url).Select(c => c.ThirdCategoryID).FirstOrDefaultAsync();

                string idToString = Convert.ToString(id);

                var result = from list in listingContext.Listing
                             join comm in listingContext.Communication
                             on list.ListingID equals comm.ListingID

                             join add in listingContext.Address
                             on list.ListingID equals add.ListingID


                             join cat in listingContext.Categories
                             .Where(c => c.ThirdCategories.Contains(idToString))
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
                                 Address = add,
                                 Categories = cat,
                                 Specialisation = spec,
                                 WorkingHours = work,
                                 PaymentMode = pay
                             };

                return View(await result.ToListAsync());
            }
            else if (level == "ivc")
            {
                int id = await categoriesContext.FourthCategory.Where(c => c.URL == url).Select(c => c.FourthCategoryID).FirstOrDefaultAsync();

                string idToString = Convert.ToString(id);

                var result = from list in listingContext.Listing
                             join comm in listingContext.Communication
                             on list.ListingID equals comm.ListingID

                             join add in listingContext.Address
                             on list.ListingID equals add.ListingID


                             join cat in listingContext.Categories
                             .Where(c => c.FourthCategories.Contains(idToString))
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
                                 Address = add,
                                 Categories = cat,
                                 Specialisation = spec,
                                 WorkingHours = work,
                                 PaymentMode = pay
                             };

                return View(await result.ToListAsync());
            }
            else if (level == "vc")
            {
                int id = await categoriesContext.FifthCategory.Where(c => c.URL == url).Select(c => c.FifthCategoryID).FirstOrDefaultAsync();

                string idToString = Convert.ToString(id);

                var result = from list in listingContext.Listing
                             join comm in listingContext.Communication
                             on list.ListingID equals comm.ListingID

                             join add in listingContext.Address
                             on list.ListingID equals add.ListingID


                             join cat in listingContext.Categories
                             .Where(c => c.FifthCategories.Contains(idToString))
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
                                 Address = add,
                                 Categories = cat,
                                 Specialisation = spec,
                                 WorkingHours = work,
                                 PaymentMode = pay
                             };

                return View(await result.ToListAsync());
            }
            else if (level == "sc")
            {
                int id = await categoriesContext.SixthCategory.Where(c => c.URL == url).Select(c => c.SixthCategoryID).FirstOrDefaultAsync();

                string idToString = Convert.ToString(id);

                var result = from list in listingContext.Listing
                             join comm in listingContext.Communication
                             on list.ListingID equals comm.ListingID

                             join add in listingContext.Address
                             on list.ListingID equals add.ListingID


                             join cat in listingContext.Categories
                             .Where(c => c.SixthCategories.Contains(idToString))
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
                                 Address = add,
                                 Categories = cat,
                                 Specialisation = spec,
                                 WorkingHours = work,
                                 PaymentMode = pay
                             };

                return View(await result.ToListAsync());
            }
            return View();
        }
    }
}