using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BAL.Listings;
using DAL.CATEGORIES;
using DAL.LISTING;
using DAL.SHARED;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRONTEND.Controllers
{
    public class CascadeDropdownCategoriesController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly SharedDbContext sharedManager;
        private readonly CategoriesDbContext categoryContext;
        private readonly IListingManager listingManager;

        public CascadeDropdownCategoriesController(ListingDbContext listingContext, SharedDbContext sharedManager, IListingManager listingManager, CategoriesDbContext categoryContext)
        {
            this.listingContext = listingContext;
            this.sharedManager = sharedManager;
            this.listingManager = listingManager;
            this.categoryContext = categoryContext;
        }
        public JsonResult fetchFirstCategory()
        {
            var selFirstCategories = categoryContext.FirstCategory
                .OrderBy(c => c.SortOrder)
                .Select(c => new { value = c.FirstCategoryID, text = c.Name });
            return Json(new SelectList(selFirstCategories, "value", "text"));
        }

        public JsonResult fetchSecondCategory(int JsonFirstCategoryID)
        {
            var selSecondCategory = categoryContext.SecondCategory
                .OrderBy(s => s.Name)
                .Where(s => s.FirstCategoryID == JsonFirstCategoryID)
                .Select(s => new { value = s.SecondCategoryID, text = s.Name });
            return Json(new SelectList(selSecondCategory, "value", "text"));
        }

        public JsonResult fetchThirdCategory(int JsonSecondCategoryID)
        {
            var selThirdCategory = categoryContext.ThirdCategory
                .OrderBy(s => s.Name)
                .Where(s => s.SecondCategoryID == JsonSecondCategoryID)
                .Select(s => new { value = s.ThirdCategoryID, text = s.Name });
            return Json(new SelectList(selThirdCategory, "value", "text"));
        }

        public JsonResult fetchFourthCategory(int JsonThirdCategoryID)
        {
            var selFourthCategory = categoryContext.FourthCategory
                .OrderBy(s => s.Name)
                .Where(s => s.ThirdCategoryID == JsonThirdCategoryID)
                .Select(s => new { value = s.FourthCategoryID, text = s.Name });
            return Json(new SelectList(selFourthCategory, "value", "text"));
        }

        public JsonResult fetchFifthCategory(int JsonFourthCategoryID)
        {
            var selFifthCategory = categoryContext.FifthCategory
                .OrderBy(s => s.Name)
                .Where(s => s.FourthCategoryID == JsonFourthCategoryID)
                .Select(s => new { value = s.FifthCategoryID, text = s.Name });
            return Json(new SelectList(selFifthCategory, "value", "text"));
        }

        public JsonResult fetchSixthCategory(int JsonFifthCategoryID)
        {
            var selSixthCategory = categoryContext.SixthCategory
                .OrderBy(s => s.Name)
                .Where(s => s.FifthCategoryID == JsonFifthCategoryID)
                .Select(s => new { value = s.SixthCategoryID, text = s.Name });
            return Json(new SelectList(selSixthCategory, "value", "text"));
        }
    }
}
