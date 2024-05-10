using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Hosting;
using BAL.Messaging.Notify;
using System.Text;

namespace FRONTEND.Controllers
{
    public class ListingController : Controller
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
        private readonly IWebHostEnvironment webHost;
        private readonly INotification notification;


        public ListingController(ListingDbContext listingContext, AuditDbContext auditContext, BillingDbContext billingContext, CategoriesDbContext categoriesContext, SharedDbContext sharedContext, ApplicationDbContext applicationContext, IHistoryAudit historyAudit, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IListingManager listingManager, IWebHostEnvironment webHost, INotification notification)
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
            this.webHost = webHost;
            this.notification = notification;
        }

        [HttpGet]
        [Route("/Listing/Index/{ListingID}")]
        public async Task<IActionResult> Index(int ListingID)
        {
            // Shafi: Get Listing Owner Guid
            string listingOwnerGuid = await listingContext.Listing.Where(l => l.ListingID == ListingID).Select(l => l.OwnerGuid).FirstOrDefaultAsync();
            // End:

            // Shafi: Get Owner Name
            ViewBag.Name = await applicationContext.UserProfile.Where(p => p.OwnerGuid == listingOwnerGuid).Select(p => p.Name).FirstOrDefaultAsync();
            // End:

            // Shafi: Get Profile ID for Profile Image
            ViewBag.ProfileID = await applicationContext.UserProfile.Where(p => p.OwnerGuid == listingOwnerGuid).Select(p => p.ProfileID).FirstOrDefaultAsync();
            // End:

            // Shafi: Get Owner Designation
            ViewBag.Designation = await listingContext.Listing.Where(l => l.ListingID == ListingID).Select(l => l.Designation).FirstOrDefaultAsync();
            // End:

            // Shafi: Set value of LikeValue hidden input in Index view
            if (await historyAudit.ListingLikeByUser(ListingID, listingOwnerGuid) == true)
            {
                ViewBag.LikeDislike = "Unlike";
            }
            else
            {
                ViewBag.LikeDislike = "Like";
            }
            // End:

            // Shafi: Set value of BookmarkValue hidden input in Index view
            if (await historyAudit.ListingBookmarkByUser(ListingID, listingOwnerGuid) == true)
            {
                ViewBag.Bookmark = "Remove-Bookmark";
            }
            else
            {
                ViewBag.Bookmark = "Bookmark";
            }
            // End:

            // Shafi: Set value of SubscribeValue hidden input in Index view
            if (await historyAudit.ListingSubscribeByUser(ListingID, listingOwnerGuid) == true)
            {
                ViewBag.Subscribe = "Unsubscribe";
            }
            else
            {
                ViewBag.Subscribe = "Subscribe";
            }
            // End:

            // Shafi: Count Likes and show in Index view
            ViewBag.LikesCounts = await auditContext.ListingLikeDislike.Where(l => l.ListingID == ListingID && l.Like == true).CountAsync();
            // End:

            // Shafi: Count Bookmarks and show in Index view
            ViewBag.BookmarkCounts = await auditContext.Bookmarks.Where(l => l.ListingID == ListingID && l.Bookmark == true).CountAsync();
            // End:

            // Shafi: Count Subscribe and show in Index view
            ViewBag.SubscribeCounts = await auditContext.Subscribes.Where(l => l.ListingID == ListingID && l.Subscribe == true).CountAsync();
            // End:

            FreeListingViewModel listing = new FreeListingViewModel();
            listing.Listing = await listingContext.Listing.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
            listing.Communication = await listingContext.Communication.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
            listing.Address = await listingContext.Address.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
            listing.Categories = await listingContext.Categories.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
            listing.Specialisation = await listingContext.Specialisation.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
            listing.PaymentMode = await listingContext.PaymentMode.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
            listing.WorkingHours = await listingContext.WorkingHours.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();

            // Shafi: Get Time Zone
            DateTime timeZoneDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            string day = timeZoneDate.ToString("dddd");
            string time = timeZoneDate.ToString("hh:mm tt");
            DateTime currentTime = DateTime.Parse(time, System.Globalization.CultureInfo.CurrentCulture);
            // End:

            if (listing.Listing != null && listing.Communication != null && listing.Address != null && listing.Categories != null && listing.Specialisation != null && listing.PaymentMode != null && listing.WorkingHours != null)
            {
                // Shafi: Get UserGuid & IP Address
                if (User.Identity.IsAuthenticated)
                {
                    IdentityUser user = await userManager.FindByNameAsync(User.Identity.Name);
                    string RemoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
                    string UserAgent = this.HttpContext.Request.Headers["User-Agent"].ToString();
                    string UserGuid = user.Id;
                    ViewBag.UserGuid = UserGuid;

                    // Shafi: Get RatingID of current loggedin user if his/her rating exists
                    var rating = await listingContext.Rating.Where(r => r.OwnerGuid == UserGuid).FirstOrDefaultAsync();
                    if(rating != null)
                    {
                        ViewBag.CurrentUserRatingID = rating.RatingID;
                    }
                    else
                    {
                        ViewBag.CurrentUserRatingID = null;
                    }
                    // End:
                }
                // End:

                // Shafi: Get rating average
                var ratingCount = await listingManager.GetRatingAsync(ListingID);

                  if (ratingCount.Count() > 0)
                {
                    var R1 = await listingManager.CountRatingAsync(ListingID, 1);
                    var R2 = await listingManager.CountRatingAsync(ListingID, 2);
                    var R3 = await listingManager.CountRatingAsync(ListingID, 3);
                    var R4 = await listingManager.CountRatingAsync(ListingID, 4);
                    var R5 = await listingManager.CountRatingAsync(ListingID, 5);

                    decimal averageCount = 5 * R5 + 4 * R4 + 3 * R3 + 2 * R2 + 1 * R1;
                    decimal weightedCount = R5 + R4 + R3 + R2 + R1;
                    decimal ratingAverage = averageCount / weightedCount;
                    ViewBag.RatingAverage = ratingAverage.ToString("0.0");

                    decimal R1Total = R1 * 15;
                    decimal R2Total = R2 * 15;
                    decimal R3Total = R3 * 15;
                    decimal R4Total = R4 * 15;
                    decimal R5Total = R5 * 15;

                    decimal SubTotalOfAllR = R1Total + R2Total + R3Total + R4Total + R5Total;

                    decimal R1Percent = (R1Total * 100) / SubTotalOfAllR;
                    decimal R2Percent = (R2Total * 100) / SubTotalOfAllR;
                    decimal R3Percent = (R3Total * 100) / SubTotalOfAllR;
                    decimal R4Percent = (R4Total * 100) / SubTotalOfAllR;
                    decimal R5Percent = (R5Total * 100) / SubTotalOfAllR;

                    ViewBag.R1 = R1Percent;
                    ViewBag.R2 = R2Percent;
                    ViewBag.R3 = R3Percent;
                    ViewBag.R4 = R4Percent;
                    ViewBag.R5 = R5Percent;
                }
                else
                {
                    ViewBag.RatingAverage = 0;
                }
                // End:

                // Shafi: Decode address for map url
                ViewBag.Company = System.Net.WebUtility.UrlEncode(listing.Listing.CompanyName);
                int LocalityID = listing.Address.LocalityID;
                ViewBag.Locality = await sharedContext.Locality.Where(l => l.LocalityID == LocalityID).Select(l => l.LocalityName).FirstOrDefaultAsync();
                // End:

                // Shafi: Business Open Now & Close
                if (day == "Monday")
                {
                    // Shafi: Display ViewBag in Index view
                    ViewBag.FromTime = listing.WorkingHours.MondayFrom.ToString("hh:mm tt");
                    ViewBag.ToTime = listing.WorkingHours.MondayTo.ToString("hh:mm tt");
                    ViewBag.OpenOn = "Tuesday";
                    // End:

                    // Shafi: Get ToTime in ("hh:mm tt") format then convert it to string then create date time from it
                    string timeString = listing.WorkingHours.MondayTo.ToString("hh:mm tt");
                    DateTime ToTime = DateTime.Parse(timeString, System.Globalization.CultureInfo.CurrentCulture);
                    // End:

                    if (currentTime > ToTime)
                    {
                        ViewBag.OpenClose = "Closed";
                    }
                    else
                    {
                        ViewBag.OpenClose = "Open Now";
                    }
                }

                if (day == "Tuesday")
                {
                    // Shafi: Display ViewBag in Index view
                    ViewBag.FromTime = listing.WorkingHours.TuesdayFrom.ToString("hh:mm tt");
                    ViewBag.ToTime = listing.WorkingHours.TuesdayTo.ToString("hh:mm tt");
                    ViewBag.OpenOn = "Wednesday";
                    // End:

                    // Shafi: Get ToTime in ("hh:mm tt") format then convert it to string then create date time from it
                    string timeString = listing.WorkingHours.TuesdayTo.ToString("hh:mm tt");
                    DateTime ToTime = DateTime.Parse(timeString, System.Globalization.CultureInfo.CurrentCulture);
                    // End:

                    if (currentTime > ToTime)
                    {
                        ViewBag.OpenClose = "Closed";
                    }
                    else
                    {
                        ViewBag.OpenClose = "Open Now";
                    }
                }

                if (day == "Wednesday")
                {
                    // Shafi: Display ViewBag in Index view
                    ViewBag.FromTime = listing.WorkingHours.WednesdayFrom.ToString("hh:mm tt");
                    ViewBag.ToTime = listing.WorkingHours.WednesdayTo.ToString("hh:mm tt");
                    ViewBag.OpenOn = "Thursday";
                    // End:

                    // Shafi: Get ToTime in ("hh:mm tt") format then convert it to string then create date time from it
                    string timeString = listing.WorkingHours.WednesdayTo.ToString("hh:mm tt");
                    DateTime ToTime = DateTime.Parse(timeString, System.Globalization.CultureInfo.CurrentCulture);
                    // End:

                    if (currentTime > ToTime)
                    {
                        ViewBag.OpenClose = "Closed";
                    }
                    else
                    {
                        ViewBag.OpenClose = "Open Now";
                    }
                }

                if (day == "Thursday")
                {
                    // Shafi: Display ViewBag in Index view
                    ViewBag.FromTime = listing.WorkingHours.ThursdayFrom.ToString("hh:mm tt");
                    ViewBag.ToTime = listing.WorkingHours.ThursdayTo.ToString("hh:mm tt");
                    ViewBag.OpenOn = "Friday";
                    // End:

                    // Shafi: Get ToTime in ("hh:mm tt") format then convert it to string then create date time from it
                    string timeString = listing.WorkingHours.ThursdayTo.ToString("hh:mm tt");
                    DateTime ToTime = DateTime.Parse(timeString, System.Globalization.CultureInfo.CurrentCulture);
                    // End:

                    if (currentTime > ToTime)
                    {
                        ViewBag.OpenClose = "Closed";
                    }
                    else
                    {
                        ViewBag.OpenClose = "Open Now";
                    }
                }

                if (day == "Friday")
                {
                    // Shafi: Display ViewBag in Index view
                    ViewBag.FromTime = listing.WorkingHours.FridayFrom.ToString("hh:mm tt");
                    ViewBag.ToTime = listing.WorkingHours.FridayTo.ToString("hh:mm tt");
                    ViewBag.OpenOn = "Saturday";
                    // End:

                    // Shafi: Get ToTime in ("hh:mm tt") format then convert it to string then create date time from it
                    string timeString = listing.WorkingHours.FridayTo.ToString("hh:mm tt");
                    DateTime ToTime = DateTime.Parse(timeString, System.Globalization.CultureInfo.CurrentCulture);
                    // End:

                    if (currentTime > ToTime)
                    {
                        ViewBag.OpenClose = "Closed";
                    }
                    else
                    {
                        ViewBag.OpenClose = "Open Now";
                    }
                }

                if (day == "Saturday")
                {
                    // Shafi: Display ViewBag in Index view
                    ViewBag.FromTime = listing.WorkingHours.SaturdayFrom.ToString("hh:mm tt");
                    ViewBag.ToTime = listing.WorkingHours.SaturdayTo.ToString("hh:mm tt");
                    if (listing.WorkingHours.SundayHoliday != true)
                    {
                        ViewBag.OpenOn = "Sunday";
                    }
                    // End:

                    // Shafi: Get ToTime in ("hh:mm tt") format then convert it to string then create date time from it
                    string timeString = listing.WorkingHours.SaturdayTo.ToString("hh:mm tt");
                    DateTime ToTime = DateTime.Parse(timeString, System.Globalization.CultureInfo.CurrentCulture);
                    // End:

                    if (currentTime > ToTime)
                    {
                        ViewBag.OpenClose = "Closed";
                    }
                    else
                    {
                        ViewBag.OpenClose = "Open Now";
                    }
                }

                if (day == "Sunday")
                {
                    // Shafi: Display ViewBag in Index view
                    ViewBag.FromTime = listing.WorkingHours.SundayFrom.ToString("hh:mm tt");
                    ViewBag.ToTime = listing.WorkingHours.SundayTo.ToString("hh:mm tt");
                    ViewBag.OpenOn = "Monday";
                    // End:

                    // Shafi: Get ToTime in ("hh:mm tt") format then convert it to string then create date time from it
                    string timeString = listing.WorkingHours.SundayTo.ToString("hh:mm tt");
                    DateTime ToTime = DateTime.Parse(timeString, System.Globalization.CultureInfo.CurrentCulture);
                    // End:

                    if (currentTime > ToTime)
                    {
                        ViewBag.OpenClose = "Closed";
                    }
                    else
                    {
                        ViewBag.OpenClose = "Open Now";
                    }
                }

                if (listing.WorkingHours.SaturdayHoliday == true && listing.WorkingHours.SundayHoliday == true)
                {
                    ViewBag.FromTime = null;
                    ViewBag.ToTime = null;
                    ViewBag.OpenOn = "Monday";
                    ViewBag.OpenClose = "Closed";
                }

                if (listing.WorkingHours.SaturdayHoliday == true && listing.WorkingHours.SundayHoliday == false)
                {
                    ViewBag.FromTime = null;
                    ViewBag.ToTime = null;
                    ViewBag.OpenOn = "Sunday";
                    ViewBag.OpenClose = "Closed";
                }

                if (listing.WorkingHours.SundayHoliday == true)
                {
                    ViewBag.FromTime = null;
                    ViewBag.ToTime = null;
                    ViewBag.OpenOn = "Monday";
                    ViewBag.OpenClose = "Closed";
                }
                // End:

                return View(listing);
            }
            else
            {
                return Redirect("/");
            }
        }

        [HttpPost]
        public async Task<JsonResult> LikeCount(int ListingID)
        {
            var result = await auditContext.ListingLikeDislike.Where(l => l.ListingID == ListingID && l.Like == true).CountAsync();
            return Json(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> LikeDislike(int? ListingID, string Like)
        {
            // Shafi: Get UserGuid & IP Address
            IdentityUser user = await userManager.FindByNameAsync(User.Identity.Name);
            string RemoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string UserAgent = this.HttpContext.Request.Headers["User-Agent"].ToString();
            string UserGuid = user.Id;
            string Email = user.Email;
            string Mobile = user.PhoneNumber;
            // End:

            // Shafi: Get user role name
            IList<string> userInRoleName = await userManager.GetRolesAsync(user);
            string UserRole = userInRoleName.FirstOrDefault();
            // End:

            // Shafi: Assign Time Zone to CreatedDate & Created Time
            DateTime timeZoneDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            // End:

            if (Like != null && ListingID != null)
            {
                // Shafi: Check if record already exists
                var likeRecordExists = await auditContext.ListingLikeDislike.Where(l => l.ListingID == ListingID.Value && l.UserGuid == UserGuid).FirstOrDefaultAsync();

                if (likeRecordExists == null)
                {
                    // Shafi: If record does not exisits & Like == Like then [CREATE NEW RECORD]
                    if (await historyAudit.ListingLikeByUser(ListingID.Value, UserGuid) == false && Like == "Like")
                    {
                        await historyAudit.CreateListingLikeDislikeAsync(ListingID.Value, UserGuid, Email, Mobile, RemoteIpAddress, UserRole, timeZoneDate.ToString("dd/MM/yyyy"), timeZoneDate.ToString("hh:mm:ss tt"), UserAgent, true);

                        // Shafi: Customer Notification
                        // Find listing and communication details
                        var listing = await listingContext.Listing.Where(l => l.ListingID == ListingID.Value).FirstOrDefaultAsync();
                        var communication = await listingContext.Communication.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
                        var currentUserProfile = await applicationContext.UserProfile.Where(p => p.OwnerGuid == user.Id).FirstOrDefaultAsync();

                        // Get current user name if profile exists
                        string likedBy = null;
                        if (currentUserProfile != null)
                        {
                            likedBy = currentUserProfile.Name;
                        }
                        else
                        {
                            likedBy = "Someone";
                        }

                        // Send Email
                        string listingUrl = "https://localhost:44314/Listing/Index/" + listing.ListingID;
                        var listingOwner = await userManager.FindByIdAsync(listing.OwnerGuid);
                        var webRoot = webHost.WebRootPath;
                        var notificationCustomer = System.IO.Path.Combine(webRoot, "Email-Templates", "Like.html");
                        string emailMsgCustomer = System.IO.File.ReadAllText(notificationCustomer, Encoding.UTF8).Replace("{company}", listing.CompanyName).Replace("{listingUrl}", listingUrl).Replace("{name}", listing.Name).Replace("{likedBy}", likedBy);
                        notification.SendEmail(listingOwner.Email, $"{likedBy} just liked your business listing {listing.CompanyName}", emailMsgCustomer);

                        // SMS
                        if (listingOwner.PhoneNumber == null)
                        {
                            notification.SendSMS(communication.Mobile, $"{likedBy} just liked your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        else
                        {
                            notification.SendSMS(listingOwner.PhoneNumber, $"{likedBy} just liked your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        // End:
                    }
                }

                if (likeRecordExists != null)
                {
                    // Shafi: If record exisits & Like == Unlike then update value of Like to [FALSE] of existing record
                    if (await historyAudit.ListingLikeByUser(ListingID.Value, UserGuid) == true && Like == "Unlike")
                    {
                        await historyAudit.EditListingLikeDislikeAsync(ListingID.Value, UserGuid, timeZoneDate.ToString("dd/MM/yyyy"), timeZoneDate.ToString("hh:mm:ss tt"), false);
                    }

                    // Shafi: If record exisits & Like == Unlike then update value of Like to [TRUE] of existing record
                    if (await historyAudit.ListingLikeByUser(ListingID.Value, UserGuid) == false && Like == "Like")
                    {
                        await historyAudit.EditListingLikeDislikeAsync(ListingID.Value, UserGuid, timeZoneDate.ToString("dd/MM/yyyy"), timeZoneDate.ToString("hh:mm:ss tt"), true);

                        // Shafi: Customer Notification
                        // Find listing and communication details
                        var listing = await listingContext.Listing.Where(l => l.ListingID == ListingID.Value).FirstOrDefaultAsync();
                        var communication = await listingContext.Communication.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
                        var currentUserProfile = await applicationContext.UserProfile.Where(p => p.OwnerGuid == user.Id).FirstOrDefaultAsync();
                        // End:

                        // Get current user name if profile exists
                        string likedBy = null;
                        if (currentUserProfile != null)
                        {
                            likedBy = currentUserProfile.Name;
                        }
                        else
                        {
                            likedBy = "Someone";
                        }

                        // Send Email
                        string listingUrl = "https://localhost:44314/Listing/Index/" + listing.ListingID;
                        var listingOwner = await userManager.FindByIdAsync(listing.OwnerGuid);
                        var webRoot = webHost.WebRootPath;
                        var notificationCustomer = System.IO.Path.Combine(webRoot, "Email-Templates", "Like.html");
                        string emailMsgCustomer = System.IO.File.ReadAllText(notificationCustomer, Encoding.UTF8).Replace("{company}", listing.CompanyName).Replace("{listingUrl}", listingUrl).Replace("{name}", listing.Name).Replace("{likedBy}", likedBy); ;
                        notification.SendEmail(listingOwner.Email, $"{likedBy} just liked your business listing {listing.CompanyName}", emailMsgCustomer);

                        // SMS
                        if (listingOwner.PhoneNumber == null)
                        {
                            notification.SendSMS(communication.Mobile, $"{likedBy} just liked your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        else
                        {
                            notification.SendSMS(listingOwner.PhoneNumber, $"{likedBy} just liked your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        // End:
                    }
                }
            }

            if (await historyAudit.ListingLikeByUser(ListingID.Value, UserGuid) == true && Like == "Like")
            {
                var likeUnlikeId = await auditContext.ListingLikeDislike.Where(i => i.UserGuid == UserGuid && i.ListingID == ListingID.Value).Select(i => i.LikeDislikeID).FirstOrDefaultAsync();

                var result = "(Unlike)-" + likeUnlikeId;

                return Json(result);
            }
            if (await historyAudit.ListingLikeByUser(ListingID.Value, UserGuid) == false && Like == "Unlike")
            {
                var likeUnlikeId = await auditContext.ListingLikeDislike.Where(i => i.UserGuid == UserGuid && i.ListingID == ListingID.Value).Select(i => i.LikeDislikeID).FirstOrDefaultAsync();

                var result = "(Like)-" + likeUnlikeId;

                return Json(result);
            }
            else
            {
                var likeUnlikeId = await auditContext.ListingLikeDislike.Where(i => i.UserGuid == UserGuid && i.ListingID == ListingID.Value).Select(i => i.LikeDislikeID).FirstOrDefaultAsync();

                var result = "(Like)-" + likeUnlikeId;

                return Json(result);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> Bookmark(int? ListingID, string Bookmark)
        {
            // Shafi: Get UserGuid & IP Address
            IdentityUser user = await userManager.FindByNameAsync(User.Identity.Name);
            string RemoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string UserAgent = this.HttpContext.Request.Headers["User-Agent"].ToString();
            string UserGuid = user.Id;
            string Email = user.Email;
            string Mobile = user.PhoneNumber;
            // End:

            // Shafi: Get user role name
            IList<string> userInRoleName = await userManager.GetRolesAsync(user);
            string UserRole = userInRoleName.FirstOrDefault();
            // End:

            // Shafi: Assign Time Zone to CreatedDate & Created Time
            DateTime timeZoneDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            // End:

            if (Bookmark != null && ListingID != null)
            {
                // Shafi: Check if record already exists
                var bookmarkExists = await auditContext.Bookmarks.Where(l => l.ListingID == ListingID.Value && l.UserGuid == UserGuid).FirstOrDefaultAsync();

                if (bookmarkExists == null)
                {
                    // Shafi: If record does not exisits & Like == Like then [CREATE NEW RECORD]
                    if (await historyAudit.ListingBookmarkByUser(ListingID.Value, UserGuid) == false && Bookmark == "Bookmark")
                    {
                        await historyAudit.CreateBookmarkAsync(ListingID.Value, UserGuid, Email, Mobile, RemoteIpAddress, UserRole, timeZoneDate.ToString("dd/MM/yyyy"), timeZoneDate.ToString("hh:mm:ss tt"), UserAgent, true);

                        // Shafi: Customer Notification -----------------
                        // Find listing and communication details
                        var listing = await listingContext.Listing.Where(l => l.ListingID == ListingID.Value).FirstOrDefaultAsync();
                        var communication = await listingContext.Communication.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
                        var currentUserProfile = await applicationContext.UserProfile.Where(p => p.OwnerGuid == user.Id).FirstOrDefaultAsync();
                        // End:

                        // Get current user name if profile exists
                        string bookmarkedBy = null;
                        if (currentUserProfile != null)
                        {
                            bookmarkedBy = currentUserProfile.Name;
                        }
                        else
                        {
                            bookmarkedBy = "Someone";
                        }

                        // Send Email
                        string listingUrl = "https://localhost:44314/Listing/Index/" + listing.ListingID;
                        var listingOwner = await userManager.FindByIdAsync(listing.OwnerGuid);
                        var webRoot = webHost.WebRootPath;
                        var notificationCustomer = System.IO.Path.Combine(webRoot, "Email-Templates", "Bookmark.html");
                        string emailMsgCustomer = System.IO.File.ReadAllText(notificationCustomer, Encoding.UTF8).Replace("{company}", listing.CompanyName).Replace("{listingUrl}", listingUrl).Replace("{name}", listing.Name).Replace("{bookmarkedBy}", bookmarkedBy); ;
                        notification.SendEmail(listingOwner.Email, $"{bookmarkedBy} bookmarked your business listing {listing.CompanyName}", emailMsgCustomer);

                        // SMS
                        if (listingOwner.PhoneNumber == null)
                        {
                            notification.SendSMS(communication.Mobile, $"{bookmarkedBy} bookmarked your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        else
                        {
                            notification.SendSMS(listingOwner.PhoneNumber, $"{bookmarkedBy} bookmarked your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        // End: ----------------------------------
                    }
                }

                if (bookmarkExists != null)
                {
                    // Shafi: If record exisits & Like == Unlike then update value of Like to [FALSE] of existing record
                    if (await historyAudit.ListingBookmarkByUser(ListingID.Value, UserGuid) == true && Bookmark == "Remove-Bookmark")
                    {
                        await historyAudit.EditBookmarkAsync(ListingID.Value, UserGuid, timeZoneDate.ToString("dd/MM/yyyy"), timeZoneDate.ToString("hh:mm:ss tt"), false);
                    }

                    // Shafi: If record exisits & Like == Unlike then update value of Like to [TRUE] of existing record
                    if (await historyAudit.ListingBookmarkByUser(ListingID.Value, UserGuid) == false && Bookmark == "Bookmark")
                    {
                        await historyAudit.EditBookmarkAsync(ListingID.Value, UserGuid, timeZoneDate.ToString("dd/MM/yyyy"), timeZoneDate.ToString("hh:mm:ss tt"), true);

                        // Shafi: Customer Notification -----------------
                        // Find listing and communication details
                        var listing = await listingContext.Listing.Where(l => l.ListingID == ListingID.Value).FirstOrDefaultAsync();
                        var communication = await listingContext.Communication.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
                        var currentUserProfile = await applicationContext.UserProfile.Where(p => p.OwnerGuid == user.Id).FirstOrDefaultAsync();
                        // End:

                        // Get current user name if profile exists
                        string bookmarkedBy = null;
                        if (currentUserProfile != null)
                        {
                            bookmarkedBy = currentUserProfile.Name;
                        }
                        else
                        {
                            bookmarkedBy = "Someone";
                        }

                        // Send Email
                        string listingUrl = "https://localhost:44314/Listing/Index/" + listing.ListingID;
                        var listingOwner = await userManager.FindByIdAsync(listing.OwnerGuid);
                        var webRoot = webHost.WebRootPath;
                        var notificationCustomer = System.IO.Path.Combine(webRoot, "Email-Templates", "Bookmark.html");
                        string emailMsgCustomer = System.IO.File.ReadAllText(notificationCustomer, Encoding.UTF8).Replace("{company}", listing.CompanyName).Replace("{listingUrl}", listingUrl).Replace("{name}", listing.Name).Replace("{bookmarkedBy}", bookmarkedBy); ;
                        notification.SendEmail(listingOwner.Email, $"{bookmarkedBy} bookmarked your business listing {listing.CompanyName}", emailMsgCustomer);

                        // SMS
                        if (listingOwner.PhoneNumber == null)
                        {
                            notification.SendSMS(communication.Mobile, $"{bookmarkedBy} bookmarked your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        else
                        {
                            notification.SendSMS(listingOwner.PhoneNumber, $"{bookmarkedBy} bookmarked your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        // End: ----------------------------------
                    }
                }
            }

            if (await historyAudit.ListingBookmarkByUser(ListingID.Value, UserGuid) == true && Bookmark == "Bookmark")
            {
                return Json("Remove-Bookmark");
            }
            if (await historyAudit.ListingBookmarkByUser(ListingID.Value, UserGuid) == false && Bookmark == "Remove-Bookmark")
            {
                return Json("Bookmark");
            }
            else
            {
                return Json("Bookmark");
            }
        }

        [HttpPost]
        public async Task<JsonResult> BookmarkCount(int ListingID)
        {
            var result = await auditContext.Bookmarks.Where(l => l.ListingID == ListingID && l.Bookmark == true).CountAsync();
            return Json(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> Subscribe(int? ListingID, string Subscribe)
        {
            // Shafi: Get UserGuid & IP Address
            IdentityUser user = await userManager.FindByNameAsync(User.Identity.Name);
            string RemoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string UserAgent = this.HttpContext.Request.Headers["User-Agent"].ToString();
            string UserGuid = user.Id;
            string Email = user.Email;
            string Mobile = user.PhoneNumber;
            // End:

            // Shafi: Get user role name
            IList<string> userInRoleName = await userManager.GetRolesAsync(user);
            string UserRole = userInRoleName.FirstOrDefault();
            // End:

            // Shafi: Assign Time Zone to CreatedDate & Created Time
            DateTime timeZoneDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            // End:

            if (Subscribe != null && ListingID != null)
            {
                // Shafi: Check if record already exists
                var subscribeExists = await auditContext.Subscribes.Where(l => l.ListingID == ListingID.Value && l.UserGuid == UserGuid).FirstOrDefaultAsync();

                if (subscribeExists == null)
                {
                    // Shafi: If record does not exisits & Subscribe == Subscribe then [CREATE NEW RECORD]
                    if (Subscribe == "Subscribe")
                    {
                        await historyAudit.CreateSubscribeAsync(ListingID.Value, UserGuid, Email, Mobile, RemoteIpAddress, UserRole, timeZoneDate.ToString("dd/MM/yyyy"), timeZoneDate.ToString("hh:mm:ss tt"), UserAgent, true);

                        // Shafi: Customer Notification -----------------
                        // Find listing and communication details
                        var listing = await listingContext.Listing.Where(l => l.ListingID == ListingID.Value).FirstOrDefaultAsync();
                        var communication = await listingContext.Communication.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
                        var currentUserProfile = await applicationContext.UserProfile.Where(p => p.OwnerGuid == user.Id).FirstOrDefaultAsync();
                        // End:

                        // Get current user name if profile exists
                        string subscribedBy = null;
                        if (currentUserProfile != null)
                        {
                            subscribedBy = currentUserProfile.Name;
                        }
                        else
                        {
                            subscribedBy = "Someone";
                        }

                        // Send Email
                        string listingUrl = "https://localhost:44314/Listing/Index/" + listing.ListingID;
                        var listingOwner = await userManager.FindByIdAsync(listing.OwnerGuid);
                        var webRoot = webHost.WebRootPath;
                        var notificationCustomer = System.IO.Path.Combine(webRoot, "Email-Templates", "Subscribe.html");
                        string emailMsgCustomer = System.IO.File.ReadAllText(notificationCustomer, Encoding.UTF8).Replace("{company}", listing.CompanyName).Replace("{listingUrl}", listingUrl).Replace("{name}", listing.Name).Replace("{subscribedBy}", subscribedBy); ;
                        notification.SendEmail(listingOwner.Email, $"{subscribedBy} subscribed your business listing {listing.CompanyName}", emailMsgCustomer);

                        // SMS
                        if (listingOwner.PhoneNumber == null)
                        {
                            notification.SendSMS(communication.Mobile, $"{subscribedBy} subscribed your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        else
                        {
                            notification.SendSMS(listingOwner.PhoneNumber, $"{subscribedBy} subscribed your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        // End: ----------------------------------

                    }
                }

                if (Subscribe != null)
                {
                    // Shafi: If record exisits & Like == Unlike then update value of Like to [FALSE] of existing record
                    if (await historyAudit.ListingSubscribeByUser(ListingID.Value, UserGuid) == true && Subscribe == "Unsubscribe")
                    {
                        await historyAudit.EditSubscribeAsync(ListingID.Value, UserGuid, timeZoneDate.ToString("dd/MM/yyyy"), timeZoneDate.ToString("hh:mm:ss tt"), false);
                    }

                    // Shafi: If record exisits & Like == Unlike then update value of Like to [TRUE] of existing record
                    if (await historyAudit.ListingSubscribeByUser(ListingID.Value, UserGuid) == false && Subscribe == "Subscribe")
                    {
                        await historyAudit.EditSubscribeAsync(ListingID.Value, UserGuid, timeZoneDate.ToString("dd/MM/yyyy"), timeZoneDate.ToString("hh:mm:ss tt"), true);

                        // Shafi: Customer Notification -----------------
                        // Find listing and communication details
                        var listing = await listingContext.Listing.Where(l => l.ListingID == ListingID.Value).FirstOrDefaultAsync();
                        var communication = await listingContext.Communication.Where(l => l.ListingID == ListingID).FirstOrDefaultAsync();
                        var currentUserProfile = await applicationContext.UserProfile.Where(p => p.OwnerGuid == user.Id).FirstOrDefaultAsync();
                        // End:

                        // Get current user name if profile exists
                        string subscribedBy = null;
                        if (currentUserProfile != null)
                        {
                            subscribedBy = currentUserProfile.Name;
                        }
                        else
                        {
                            subscribedBy = "Someone";
                        }

                        // Send Email
                        string listingUrl = "https://localhost:44314/Listing/Index/" + listing.ListingID;
                        var listingOwner = await userManager.FindByIdAsync(listing.OwnerGuid);
                        var webRoot = webHost.WebRootPath;
                        var notificationCustomer = System.IO.Path.Combine(webRoot, "Email-Templates", "Subscribe.html");
                        string emailMsgCustomer = System.IO.File.ReadAllText(notificationCustomer, Encoding.UTF8).Replace("{company}", listing.CompanyName).Replace("{listingUrl}", listingUrl).Replace("{name}", listing.Name).Replace("{subscribedBy}", subscribedBy); ;
                        notification.SendEmail(listingOwner.Email, $"{subscribedBy} subscribed your business listing {listing.CompanyName}", emailMsgCustomer);

                        // SMS
                        if (listingOwner.PhoneNumber == null)
                        {
                            notification.SendSMS(communication.Mobile, $"{subscribedBy} subscribed your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        else
                        {
                            notification.SendSMS(listingOwner.PhoneNumber, $"{subscribedBy} subscribed your business listing {listing.CompanyName}. Open listing {listingUrl} for any help call us on 7700995500.");
                        }
                        // End: ----------------------------------
                    }
                }
            }

            if (await historyAudit.ListingSubscribeByUser(ListingID.Value, UserGuid) == true && Subscribe == "Subscribe")
            {
                return Json("Unsubscribe");
            }
            if (await historyAudit.ListingSubscribeByUser(ListingID.Value, UserGuid) == false && Subscribe == "Unsubscribe")
            {
                return Json("Subscribe");
            }
            else
            {
                return Json("Subscribe");
            }
        }

        [HttpPost]
        public async Task<JsonResult> SubscribeCount(int ListingID)
        {
            var result = await auditContext.Subscribes.Where(l => l.ListingID == ListingID && l.Subscribe == true).CountAsync();
            return Json(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> Rating(int ListingID, int Ratings, string Comment)
        {
            // Shafi: Get UserGuid & IP Address
            IdentityUser user = await userManager.FindByNameAsync(User.Identity.Name);
            string RemoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string OwnerGuid = user.Id;
            // End:

            // Shafi: Assign Time Zone to CreatedDate & Created Time
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            // End:

            if (await listingManager.ReviewExistsAsync(ListingID, OwnerGuid) == true)
            {
                // Shafi: Show message through ajax with json data
                string data = "Sorry! You already posted a review for this listing.";
                // End:
                return Json(data);
            }
            else
            {
                // Shafi: Create rating
                await listingManager.CreateRatingAsync(ListingID, OwnerGuid, RemoteIpAddress, dateTime, dateTime, Ratings, Comment, user.Email);
                // End:

                // Shafi: Show message through ajax with json data
                string data = "Review submitted successfully.";
                // End:
                return Json(data);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> EditRating(int RatingID, string EditComment, int EditStars)
        {
            // Shafi: Get UserGuid & IP Address
            IdentityUser user = await userManager.FindByNameAsync(User.Identity.Name);
            string RemoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string OwnerGuid = user.Id;
            // End:

            // Shafi: Assign Time Zone to CreatedDate & Created Time
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            // End:

            // Shafi: Find rating for current user by RatingID
            var rating = await listingContext.Rating.Where(r => r.RatingID == RatingID && r.OwnerGuid == OwnerGuid).FirstOrDefaultAsync();
            var ratingID = rating.RatingID;
            string ratingOwnerGuid = rating.OwnerGuid;
            var listingID = rating.ListingID;
            // End:

            if (rating != null)
            {
                rating.Comment = EditComment;
                rating.Ratings = EditStars;
                rating.Date = dateTime;
                rating.Time = dateTime;
                rating.IPAddress = RemoteIpAddress;
                listingContext.Update(rating);
                await listingContext.SaveChangesAsync();

                // Shafi: Show message through ajax with json data
                string data = "Thank you for updating your rating for this listing.";
                // End:
                return Json(data);
            }
            else
            {
                // Shafi: Show message through ajax with json data
                string data = "Opps! Something went wrong, please try again.";
                // End:
                return Json(data);
            }
        }

        [HttpPost]
        public IActionResult EditRatingViewComponent(int RatingID)
        {
            return ViewComponent("RatingEdit", new { RatingID = RatingID });
        }

        [HttpPost]
        public async Task<IActionResult> ReviewsPartialView(int ListingID)
        {
            if (User.Identity.IsAuthenticated == true)
            {
                // Shafi: Get UserGuid
                IdentityUser user = await userManager.FindByNameAsync(User.Identity.Name);
                string UserGuid = user.Id;
                // End:

                // Shafi: Check if listing contains rating of current user
                bool userHaveRating = listingContext.Rating.Any(r => r.OwnerGuid == UserGuid);
                // End:

                // Shafi: Count all ratings for this listing and get id of first or default rating
                var ratingCount = await listingContext.Rating.Where(r => r.ListingID == ListingID).CountAsync();
                var latestRatingId = await listingContext.Rating.Where(r => r.ListingID == ListingID).OrderByDescending(r => r.RatingID).Select(r => r.RatingID).FirstOrDefaultAsync();
                // End:

                // Shafi: If user have rating then return all rating except this user's rating
                if (userHaveRating == true)
                {
                    // Shafi: Get rating for current user
                    var userRatingId = await listingContext.Rating.Where(r => r.OwnerGuid == UserGuid).Select(r => r.RatingID).FirstOrDefaultAsync();
                    // End:

                    // Shafi: Check if count of listing is <= 1
                    if (ratingCount <= 1)
                    {
                        // Shafi: Skip user rating then return rating model
                        var ratings = await listingContext.Rating.Where(r => r.ListingID == ListingID && r.RatingID != latestRatingId && r.RatingID != userRatingId).OrderByDescending(r => r.RatingID).ToListAsync();
                        // End:

                        // Shafi: Return partial view through ajax call in Index view
                        return PartialView("_ReviewsAndRatingsPartial", ratings);
                        // End:
                    }
                    // End:
                    else
                    {
                        // Shafi: Skip user rating then return rating model
                        var ratings = await listingContext.Rating.Where(r => r.ListingID == ListingID && r.RatingID != userRatingId).OrderByDescending(r => r.RatingID).ToListAsync();
                        // End:

                        // Shafi: Return partial view through ajax call in Index view
                        return PartialView("_ReviewsAndRatingsPartial", ratings);
                        // End:
                    }
                }
                // End:
                else
                {
                    var ratings = await listingContext.Rating.Where(r => r.ListingID == ListingID).OrderByDescending(r => r.RatingID).Skip(1).ToListAsync();

                    // Shafi: Return partial view through ajax call in Index view
                    return PartialView("_ReviewsAndRatingsPartial", ratings);
                    // End:
                }
            }
            else
            {
                // Shafi: Count all ratings for this listing and get id of first or default rating
                var ratingCount = await listingContext.Rating.Where(r => r.ListingID == ListingID).CountAsync();
                var latestRatingId = await listingContext.Rating.Where(r => r.ListingID == ListingID).OrderByDescending(r => r.RatingID).Select(r => r.RatingID).FirstOrDefaultAsync();
                // End:

                var ratings = await listingContext.Rating.Where(r => r.ListingID == ListingID).OrderByDescending(r => r.RatingID).Skip(1).ToListAsync();

                // Shafi: Return partial view through ajax call in Index view
                return PartialView("_ReviewsAndRatingsPartial", ratings);
                // End:
            }
        }

        [HttpPost]
        public IActionResult ReviewByUserOnTopViewComponent(int ListingID)
        {
            return ViewComponent("ReviewByUserOnTop", new { ListingID = ListingID });
        }

        [HttpPost]
        public IActionResult RatingBarsViewComponent(int ListingID)
        {
            return ViewComponent("RatingBars", new { ListingID = ListingID });
        }
    }
}
