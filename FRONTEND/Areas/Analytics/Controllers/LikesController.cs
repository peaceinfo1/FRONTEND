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
using BOL.AUDITTRAIL;
using DAL.AUDIT;
using BOL.VIEWMODELS;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.Analytics.Controllers
{
    [Area("Analytics")]
    [Authorize]
    public class LikesController : Controller
    {
        private readonly AuditDbContext auditContext;
        private readonly ListingDbContext listingContext;
        private readonly IUserService _userService;

        public LikesController(AuditDbContext auditContext, ListingDbContext listingContext, IUserService userService)
        {
            this.auditContext = auditContext;
            this.listingContext = listingContext;
            this._userService = userService;
        }

        // GET: Analytics/Likes
        public async Task<IActionResult> Index()
        {
            // Shafi: Get user guid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string UserGuid = user.Id;
            // End:

            // Shafi: Begin partial view model BookmarkListingViewModels
            var likeList = await auditContext.ListingLikeDislike.Where(b => b.UserGuid == UserGuid && b.Like == true).ToListAsync();
            var likeListingIDs = likeList.Select(b => b.ListingID).ToList();
            var listingList = await listingContext.Listing.Where(l => likeListingIDs.Contains(l.ListingID)).ToListAsync();
            // End:

            // Shafi: Join Bookarks and Listing
            var likeModal = (from like in likeList
                                 join listing in listingList
                         on like.ListingID equals listing.ListingID
                                 select new LikeListingViewModel
                                 {
                                     LikeDislikeID = like.LikeDislikeID,
                                     ListingID = listing.ListingID,
                                     CompanyName = listing.CompanyName,
                                     VisitDate = like.VisitDate.ToString(),
                                     VisitTime = like.VisitTime.ToString(),
                                 }).ToList();
            // End:

            return View(likeModal);
        }
    }
}
