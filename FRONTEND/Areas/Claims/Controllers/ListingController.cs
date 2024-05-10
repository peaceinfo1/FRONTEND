using BAL.Claims.Listing;
using DAL.AUDIT;
using DAL.LISTING;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.Claims.Controllers
{
    [Area("Claims")]
    public class ListingController : Controller
    {
        public IClaimListing ClaimListing;
        public IUserService _userService;
        public AuditDbContext AuditContext;
        public ListingDbContext ListingContext;
        private readonly IWebHostEnvironment HostingEnvironment;

        public ListingController(IClaimListing claimListing, IUserService userService, AuditDbContext auditContext, 
            ListingDbContext listingContext, IWebHostEnvironment hostingEnvironment)
        {
            ClaimListing = claimListing;
            _userService = userService;
            AuditContext = auditContext;
            ListingContext = listingContext;
            HostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> Index(int ListingId)
        {
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            ViewBag.UserGuid = user.Id;

            HttpContext.Session.SetString("ListingId", ListingId.ToString());
            HttpContext.Session.SetString("UserGuid", user.Id);

            ViewBag.UserGuid = HttpContext.Session.GetString("UserGuid");
            ViewBag.ListingID = HttpContext.Session.GetString("ListingId");
            return View();
        }

        public IActionResult OtpBySms()
        {
            ViewBag.UserGuid = HttpContext.Session.GetString("UserGuid");
            ViewBag.ListingID = HttpContext.Session.GetString("ListingId");
            return View();
        }

        public IActionResult OtpByEmail()
        {
            ViewBag.UserGuid = HttpContext.Session.GetString("UserGuid");
            ViewBag.ListingID = HttpContext.Session.GetString("ListingId");
            return View();
        }

        public IActionResult ByUploadDocument()
        {
            ViewBag.UserGuid = HttpContext.Session.GetString("UserGuid");
            ViewBag.ListingID = HttpContext.Session.GetString("ListingId");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ByUploadDocument(IFormFile documentOne, IFormFile documentTwo, IFormFile documentThree, string message)
        {
            ViewBag.UserGuid = HttpContext.Session.GetString("UserGuid");
            ViewBag.ListingID = HttpContext.Session.GetString("ListingId");

            int documentCount = 0;

            if (documentOne != null)
            {
                string folderListingId = HostingEnvironment.ContentRootPath + $"\\AppData\\{ViewBag.ListingID}\\";
                
                if(!Directory.Exists(folderListingId))
                {
                    Directory.CreateDirectory(folderListingId);
                }

                var extension = Path.GetExtension(documentOne.FileName);

                if (extension == ".jpg" || extension == ".jpeg")
                {
                    var renameFile = "DocumentOne" + extension;
                    if (documentOne.Length > 0)
                        using (var fileStream = new FileStream(Path.Combine(folderListingId, renameFile), FileMode.Create))
                            await documentOne.CopyToAsync(fileStream);
                }

                documentCount++;
            }

            if (documentTwo != null)
            {
                string folderListingId = HostingEnvironment.ContentRootPath + $"\\AppData\\{ViewBag.ListingID}\\";

                if (!Directory.Exists(folderListingId))
                {
                    Directory.CreateDirectory(folderListingId);
                }

                var extension = Path.GetExtension(documentTwo.FileName);

                if (extension == ".jpg" || extension == ".jpeg")
                {
                    var renameFile = "DocumentTwo" + extension;
                    if (documentTwo.Length > 0)
                        using (var fileStream = new FileStream(Path.Combine(folderListingId, renameFile), FileMode.Create))
                            await documentTwo.CopyToAsync(fileStream);
                }

                documentCount++;
            }

            if (documentThree != null)
            {
                string folderListingId = HostingEnvironment.ContentRootPath + $"\\AppData\\{ViewBag.ListingID}\\";

                if (!Directory.Exists(folderListingId))
                {
                    Directory.CreateDirectory(folderListingId);
                }

                var extension = Path.GetExtension(documentThree.FileName);

                if (extension == ".jpg" || extension == ".jpeg")
                {
                    var renameFile = "DocumentThree" + extension;
                    if (documentThree.Length > 0)
                        using (var fileStream = new FileStream(Path.Combine(folderListingId, renameFile), FileMode.Create))
                            await documentThree.CopyToAsync(fileStream);
                }

                documentCount++;
            }

            await ClaimListing.GenerateDocumentOTP(ViewBag.UserGuid, Int32.Parse(ViewBag.ListingID), message);

            TempData["Message"] = $"Please select {documentCount} document to upload.";
            return RedirectToAction("DocumentUploadSuccessfull", "Listing", "Claims");
        }

        public async Task<IActionResult> DocumentUploadSuccessfull()
        {
            ViewBag.UserGuid = HttpContext.Session.GetString("UserGuid");
            ViewBag.ListingID = HttpContext.Session.GetString("ListingId");
            int listingId = Int32.Parse(ViewBag.ListingID);

            ViewBag.ListingCompanyName = await ListingContext.Listing.Where(i => i.ListingID == listingId).Select(i => i.CompanyName).FirstOrDefaultAsync();

            return View();
        }

        [HttpGet]
        [Route("/Claims/Listing/DocumentOtp/{shortLink}")]
        public async Task<IActionResult> DocumentOtp(string shortLink)
        {
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            var claim = await AuditContext.ListingClaim.Where(i => i.ClaimVerificationShortLink == shortLink).FirstOrDefaultAsync();

            if(claim == null)
            {
                return NotFound();
            }

            if(claim.ClaimedBy == user.Id)
            {
                ViewBag.ShortLink = shortLink;
                return View();
            }
            else
            {
                return Redirect($"/cvl/{shortLink}");
            }
        }

        [HttpPost]
        public async Task<JsonResult> VerifyDocumentOtp(string shortLink, int? otp)
        {
            var user = await _userService.GetUserByUserName(User.Identity.Name);

            if(shortLink != "" && otp != null)
            {
                var claim = await AuditContext.ListingClaim.Where(i => i.ClaimVerificationShortLink == shortLink).FirstOrDefaultAsync();

                if(claim != null)
                {
                    if(claim.OTP == otp)
                    {
                        var result = await ClaimListing.VerifyDocumentOTP(shortLink, user.PhoneNumber, user.Email);
                        var json = JsonConvert.SerializeObject(result);
                        return Json(json);
                    }
                    else
                    {
                        return Json("Fail");
                    }
                }
                else
                {
                    return Json("Fail");
                }
            }
            else
            {
                return Json("Fail");
            }
        }

        public async Task<IActionResult> Successfull()
        {
            ViewBag.UserGuid = HttpContext.Session.GetString("UserGuid");
            ViewBag.ListingID = HttpContext.Session.GetString("ListingId");
            int listingId = Int32.Parse(ViewBag.ListingID);

            ViewBag.ListingCompanyName = await ListingContext.Listing.Where(i => i.ListingID == listingId).Select(i => i.CompanyName).FirstOrDefaultAsync();

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> CheckIfUserAlreadyClaimedAnyListing(string userGuid)
        {
            return Json(await ClaimListing.CheckIfUserAlreadyClaimedAnyListing(userGuid));
        }

        [HttpPost]
        public async Task<JsonResult> GenerateMobileOTP(string mobileNumber, string userGuid, int listingId, string message)
        {
            return Json(await ClaimListing.GenerateMobileOTP(mobileNumber, userGuid, listingId, message));
        }

        [HttpPost]
        public async Task<JsonResult> GenerateEmailOTP(string email, string userGuid, int listingId, string message)
        {
            return Json(await ClaimListing.GenerateEmailOTP(email, userGuid, listingId, message));
        }

        [HttpPost]
        public async Task<JsonResult> VerifyMobileOTP(string mobileNumber, string userGuid, int listingId, int otp)
        {
            var claim = await AuditContext.ListingClaim.Where(i => i.ListingID == listingId && i.Mobile == mobileNumber && i.ClaimedBy == userGuid).OrderByDescending(i => i.ClaimID).FirstOrDefaultAsync();

            if (claim == null)
            {
                return Json("Invalid OTP, please try to claim listing again.");
            }
            else
            {
                var result = await ClaimListing.VerifyMobileOTP(mobileNumber, userGuid, listingId, otp);
                var json = JsonConvert.SerializeObject(result);
                return Json(json);
            }
        }

        [HttpPost]
        public async Task<JsonResult> VerifyEmailOTP(string email, string userGuid, int listingId, int otp)
        {
            var claim = await AuditContext.ListingClaim.Where(i => i.ListingID == listingId && i.Email == email && i.ClaimedBy == userGuid).OrderByDescending(i => i.ClaimID).FirstOrDefaultAsync();

            if (claim == null)
            {
                return Json("Invalid OTP, please try to claim listing again.");
            }
            else
            {
                var result = await ClaimListing.VerifyEmailOTP(email, userGuid, listingId, otp);

                var json = JsonConvert.SerializeObject(result);

                return Json(json);
            }
        }
    }
}
