using System.Linq;
using System.Threading.Tasks;
using BAL.Listings;
using DAL.CATEGORIES;
using DAL.LISTING;
using DAL.SHARED;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FRONTEND.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly SharedDbContext sharedManager;
        private readonly IListingManager listingManager;
        private readonly SharedDbContext sharedContext;
        private readonly CategoriesDbContext categoryContext;

        public CategoriesController(ListingDbContext listingContext, SharedDbContext sharedManager, IListingManager listingManager, SharedDbContext sharedContext, CategoriesDbContext categoryContext)
        {
            this.listingContext = listingContext;
            this.sharedManager = sharedManager;
            this.listingManager = listingManager;
            this.sharedContext = sharedContext;
            this.categoryContext = categoryContext;
        }

        [Route("/Third/{secondCatUrl}")]
        [HttpGet]
        public async Task<IActionResult> Third(string secondCatUrl)
        {
            var modal = await categoryContext.ThirdCategory.Where(c => c.SecondCategory.URL == secondCatUrl).ToListAsync();
            return View(modal);
        }

        [Route("/Fourth/{thirdCatUrl}")]
        [HttpGet]
        public async Task<IActionResult> Fourth(string thirdCatUrl)
        {
            var modal = await categoryContext.FourthCategory.Where(c => c.ThirdCategory.URL == thirdCatUrl).ToListAsync();
            return View(modal);
        }

        [Route("/Fifth/{fourthCatUrl}")]
        [HttpGet]
        public async Task<IActionResult> Fifth(string fourthCatUrl)
        {
            var modal = await categoryContext.FifthCategory.Where(c => c.FourthCategory.URL == fourthCatUrl).ToListAsync();
            return View(modal);
        }

        [Route("/Sixth/{fifthCatUrl}")]
        [HttpGet]
        public async Task<IActionResult> Sixth(string fifthCatUrl)
        {
            var modal = await categoryContext.SixthCategory.Where(c => c.FifthCategory.URL == fifthCatUrl).ToListAsync();
            return View(modal);
        }
    }
}
