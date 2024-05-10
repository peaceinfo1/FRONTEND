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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using BAL.Listings;
using BAL.Audit;
using BAL.Messaging.Contracts;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using System.Text.Encodings.Web;
using BAL.Services.Contracts;
using System.Xml.Linq;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
    [Authorize]
    public class PaymentModesController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly IUserService _userService;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;
        private readonly IMessageMailService notification;

        public PaymentModesController(ListingDbContext listingContext, IUserService userService, 
            IHistoryAudit audit, IListingManager listingManager, IMessageMailService notification)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.listingManager = listingManager;
            this.audit = audit;
            this.notification = notification;
        }

        // GET: Subscriptions/PaymentModes/Create
        public IActionResult Create()
        {
            // Shafi: Check if user created the listing recently
            if (HttpContext.Session.GetInt32("ListingID") == null)
            {
                return RedirectToAction("Index", "Listings", "Subscriptions");
            }
            // End:

            // Shafi: Get listing id from session
            ViewBag.ListingID = HttpContext.Session.GetInt32("ListingID");
            // End:

            return View();
        }

        // POST: Subscriptions/PaymentModes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PaymentID,ListingID,OwnerGuid,IPAddress,Cash,NetBanking,Cheque,RtgsNeft,DebitCard,CreditCard,PayTM,PhonePay,Paypal")] PaymentMode paymentMode)
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
            paymentMode.OwnerGuid = ownerGuid;
            paymentMode.IPAddress = remoteIpAddress;
            paymentMode.ListingID = HttpContext.Session.GetInt32("ListingID").Value;
            // End:

            if (ModelState.IsValid)
            {
                listingContext.Add(paymentMode);
                await listingContext.SaveChangesAsync();

                // Shafi: Display popup after creating listing
                TempData["ListingCreatedSuccessfully"] = "Contratulation! Listing Created Successfully. Your listing is under review. We will notify you within 48 hours as we approve it.";
                // End:

                try
                {

                    if (listingContext != null && listingContext.Listing != null && paymentMode != null)
                    {
                        var listing = await listingContext.Listing.Where(l => l.ListingID == paymentMode.ListingID).FirstOrDefaultAsync();

                        // Continue processing with the 'listing' variable...
                    }
                    else
                    {
                        // Handle the case where one of the objects is null
                        // Log an error, throw an exception, or take appropriate action
                    }



                    //if (listingContext.Listing != null)
                    //{
                    //    var listing = await listingContext.Listing.Where(l => l.ListingID == paymentMode.ListingID).FirstOrDefaultAsync();
                    //    var communication = await listingContext.Communication.Where(l => l.ListingID == paymentMode.ListingID).FirstOrDefaultAsync();
                    //}


                }
                catch (Exception ex)
                {

                }

                //Shafi: Find listing and communication details
                //  var listing = await listingContext.Listing.Where(l => l.ListingID == paymentMode.ListingID).FirstOrDefaultAsync();
                //  var communication = await listingContext.Communication.Where(l => l.ListingID == paymentMode.ListingID).FirstOrDefaultAsync();
                //End:

                //Shafi: Temporarlity disabled notifications in listing ziward
                //     performed action as per the request received by client
                //     after creating softawre control panel enable notification
                //     Send SMS

                // Shafi: Send notification email to User
                // Get newsletter template for user
                //    string listingUrl = "https://localhost:44314/Listing/Index/" + listing.ListingID;
                //    var webRoot = webHost.WebRootPath;
                //    var notificationTemplate = System.IO.Path.Combine(webRoot, "Email-Templates", "Free-Listing.html");
                //    string emailMsgUser = System.IO.File.ReadAllText(notificationTemplate, Encoding.UTF8).Replace("{name}", listing.Name).Replace("{company}", listing.CompanyName).Replace("{listingUrl}", listingUrl);
                //    notification.SendEmail(user.Email, $"Listing of {listing.CompanyName} Created Successfully", emailMsgUser);
                //End:

                //Shafi: Get newsletter template for self
                //    var notificationTemplateSelf = System.IO.Path.Combine(webRoot, "Email-Templates", "Free-Listing-Self.html");
                //    string emailMsgSelf = System.IO.File.ReadAllText(notificationTemplateSelf, Encoding.UTF8).Replace("{name}", listing.Name).Replace("{company}", listing.CompanyName).Replace("{listingUrl}", listingUrl).Replace("{mobile}", communication.Mobile).Replace("{email}", user.Email);
                //if (listing.Approved == true)
                //{
                //    notification.SendEmail("myinteriormart@gmail.com", $"New Listing Created: {listing.CompanyName}", emailMsgSelf);
                //}
                //End:


                //    if (user.PhoneNumber != null)
                //    {
                //        if (user.PhoneNumber == communication.Mobile)
                //        {
                //            notification.SendSMS(user.PhoneNumber, $"Dear {listing.Name}, Listing of {listing.CompanyName} created, {listingUrl}, it will be live within 48 hours. For Help Call 7700995500.");

                //            notification.SendSMS("9819007720", $"Notification: {listing.Name} created a new listing for {listing.CompanyName}, view listing {listingUrl} or call user at {user.PhoneNumber}. Approval required to publish it.");
                //        }

                //        if (user.PhoneNumber != communication.Mobile)
                //        {
                //            string mobile = communication.Mobile;
                //            notification.SendSMS(mobile, $"Dear {listing.Name}, thank you for creating listing of {listing.CompanyName} click here to view {listingUrl} & to claim this listing or make changes call us on 7700995500.");
                //        }
                //    }

                //    if (user.PhoneNumber == null)
                //    {
                //        notification.SendSMS("9819007720", $"Notification: {listing.Name} created a new listing for {listing.CompanyName}, view listing {listingUrl} or call user at {communication.Mobile} & required your approval to publish.");
                //    }
                //End:

                //return RedirectToAction("LogoAndThumbnail", "ListingFileManager", "Subscriptions");
                return RedirectToAction("Create");
                //return View(paymentMode);
            }


            //ViewBag.Message = string.Format("Contratulation! Listing Created Successfully. Your listing is under review. We will notify you within 48 hours as we approve it.");
            //return View();
            
            return View(paymentMode);
        }

        // GET: Subscriptions/PaymentModes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string userGuid = user.Id;
            // End:

            if (id == null)
            {
                return NotFound();
            }

            var paymentMode = await listingContext.PaymentMode.FindAsync(id);
            if (paymentMode == null)
            {
                return NotFound();
            }

            // Shafi: Verify record ownership
            var owner = await listingManager.PaymentOwnerAsync(paymentMode.PaymentID, paymentMode.ListingID, userGuid);
            if (owner == true)
            {
                return View(paymentMode);
            }
            else
            {
                return NotFound();
            }
            // End:
        }

        // POST: Subscriptions/PaymentModes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentID,ListingID,OwnerGuid,IPAddress,Cash,NetBanking,Cheque,RtgsNeft,DebitCard,CreditCard,PayTM,PhonePay,Paypal")] PaymentMode paymentMode)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string userGuid = user.Id;
            // End:

            if (id != paymentMode.PaymentID)
            {
                return NotFound();
            }

            // Shafi: Verify ownership
            var owner = await listingManager.PaymentOwnerAsync(paymentMode.PaymentID, paymentMode.ListingID, userGuid);
            if (owner == true)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        listingContext.Update(paymentMode);
                        await listingContext.SaveChangesAsync();
                        // Shafi: Create last updated
                        string ipAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
                        string userAgent = this.HttpContext.Request.Headers["User-Agent"];
                        string referUrl = this.HttpContext.Request.Headers["Referer"];
                        string visitedURL = this.HttpContext.Request.Headers["Host"];
                        string email = user.Email;
                        string mobile = user.PhoneNumber;
                        DateTime timeZoneDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        string updatedDate = timeZoneDate.ToString("d-MM-yyyy");
                        string updatedTime = timeZoneDate.ToString("hh:mm:ss tt");
                        string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/Subscriptions/PaymentModes/Edit/" + paymentMode.ListingID;
                        string activity = "Updated payment mode details with id " + paymentMode.PaymentID;

                        // Shafi: Get user in roles
                        IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                        string roleName = userInRoleName.FirstOrDefault();
                        // End:

                        await audit.CreateListingLastUpdatedAsync(paymentMode.PaymentID, userGuid, email, mobile, ipAddress, roleName, "Payment Modes", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                        // End:
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!PaymentModeExists(paymentMode.PaymentID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    return RedirectToAction("Index", "Listings", "Subscriptions");

                }
            }
            else
            {
                return NotFound();
            }

            return View(paymentMode);
        }

        private bool PaymentModeExists(int id)
        {
            return listingContext.PaymentMode.Any(e => e.PaymentID == id);
        }
    }
}
