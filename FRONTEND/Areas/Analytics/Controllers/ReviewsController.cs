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
using DAL.SHARED;
using Microsoft.AspNetCore.Identity;
using BAL.Audit;
using BAL.Listings;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.Analytics.Controllers
{
    [Area("Analytics")]
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly IUserService _userService;

        public ReviewsController(ListingDbContext listingContext, IUserService userService)
        {
            this.listingContext = listingContext;
            this._userService = userService;
        }

        // GET: Analytics/Reviews
        public async Task<IActionResult> Index()
        {
            // Shafi: Get user guid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string UserGuid = user.Id;
            // End:

            return View(await listingContext.Rating.Where(r => r.OwnerGuid == UserGuid).OrderByDescending(r => r.RatingID).ToListAsync());
        }
    }
}
