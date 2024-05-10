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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using DAL.SHARED;
using BAL.Listings;
using Microsoft.AspNetCore.Identity.UI.Services;
using BAL.Messaging.Contracts;
using Hangfire;
using BAL.Audit;
using DAL.AUDIT;
using Org.BouncyCastle.Math.EC.Rfc7748;
using BOL.VIEWMODELS.Dashboards;
using System.Text;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
    [Authorize]
    public class ListingDashboardController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly AuditDbContext auditContext;
        private readonly IUserService _userService;

        public ListingDashboardController(ListingDbContext listingContext, IUserService userService, AuditDbContext auditContext)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.auditContext = auditContext;
        }

        [HttpGet]
        [Route("/Subscriptions/ListingDashboard/{ListingID}")]
        public async Task<IActionResult> Index(int? ListingID /*string company*/)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string OwnerGuid = user.Id;
            // End:

            // Shafi: Count likes
            ViewBag.Likes = await auditContext.ListingLikeDislike.Where(l => l.ListingID == ListingID && l.Like == true).CountAsync();
            // End:

            // Shafi: Count bookmarks
            ViewBag.Bookmarks = await auditContext.Bookmarks.Where(l => l.ListingID == ListingID && l.Bookmark == true).CountAsync();
            // End:

            // Shafi: Count subscribe
            ViewBag.Subscribes = await auditContext.Subscribes.Where(l => l.ListingID == ListingID && l.Subscribe == true).CountAsync();
            // End:

            // Shafi: Count reviews
            ViewBag.Reviews = await listingContext.Rating.Where(l => l.ListingID == ListingID).CountAsync();
            // End:

            // Shafi: Count view counts
            ViewBag.ListingViewCount = await listingContext.ListingViewCount.Where(l => l.ListingID == ListingID).Select(l => l.ViewCount).FirstOrDefaultAsync();
            // End:

            ViewBag.ListingID = ListingID;
            return View();
        }

        [Route("/Subscriptions/ListingDashboard/Reviews/{listingId}")]
        public async Task<IActionResult> Reviews(int listingId)
        {
            return View(await listingContext.Rating.Where(r => r.ListingID == listingId).OrderByDescending(r => r.RatingID).ToListAsync());
        }

        [Route("/Subscriptions/ListingDashboard/Views/{listingId}")]
        public async Task<IActionResult> Views(int listingId)
        {
            var result = await listingContext.ListingViews.Where(x => x.ListingID == listingId).OrderByDescending(x => x.ListingViewID).ToListAsync();
            return View(result);
        }

        [Route("/Subscriptions/ListingDashboard/Likes/{listingId}")]
        public async Task<IActionResult> Likes(int listingId)
        {
            var result = await auditContext.ListingLikeDislike.Where(x => x.ListingID == listingId).OrderByDescending(x => x.LikeDislikeID).ToListAsync();
            return View(result);
        }

        [Route("/Subscriptions/ListingDashboard/Bookmarks/{listingId}")]
        public async Task<IActionResult> Bookmarks(int listingId)
        {
            var result = await auditContext.Bookmarks.Where(x => x.ListingID == listingId).OrderByDescending(x => x.BookmarksID).ToListAsync();
            return View(result);
        }

        [Route("/Subscriptions/ListingDashboard/Subscribes/{listingId}")]
        public async Task<IActionResult> Subscribes(int listingId)
        {
            var result = await auditContext.Subscribes.Where(x => x.ListingID == listingId).OrderByDescending(x => x.SubscribeID).ToListAsync();
            return View(result);
        }
    }
}
