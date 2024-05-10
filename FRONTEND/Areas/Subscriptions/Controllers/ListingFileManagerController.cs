using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
    [Authorize]
    public class ListingFileManagerController : Controller
    {
        private readonly IWebHostEnvironment hostingEnvironment;
        public ListingFileManagerController(IWebHostEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public IActionResult LogoAndThumbnail()
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

        [HttpPost]
        public async Task<IActionResult> UploadListingLogo(int? listingId, IFormFile file)
        {
            if(file != null && listingId != null)
            {
                string directoryPath = hostingEnvironment.WebRootPath + "\\FileManager\\ListingLogo\\";

                var extension = Path.GetExtension(file.FileName);

                if(extension == ".jpg")
                {
                    var renameFile = listingId + extension;

                    if (file.Length > 0)
                        using (var fileStream = new FileStream(Path.Combine(directoryPath, renameFile), FileMode.Create))
                            await file.CopyToAsync(fileStream);

                    return Redirect(Request.Headers["referer"]);
                }
                else
                {
                    TempData["RefererUrl"] = Request.Headers["referer"].ToString();
                    TempData["Message"] = "Unsupported file format.";
                    TempData["Description"] = $"The logo image which you are trying to upload is in {extension} format. Please upload your logo in *.jpg image format only with dimentions 300 x 300 pixels.";

                    return RedirectToAction("ImageFormatNotSupported", "ErrorsAndExceptions", "Subscriptions");

                }
            }
            else
            {
                return Redirect(Request.Headers["referer"]);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadListingThumbnail(int? listingId, IFormFile file)
        {
            if (file != null && listingId != null)
            {
                string directoryPath = hostingEnvironment.WebRootPath + "\\FileManager\\ListingThumbnail\\";

                var extension = Path.GetExtension(file.FileName);

                if (extension == ".jpg")
                {
                    var renameFile = listingId + extension;

                    if (file.Length > 0)
                        using (var fileStream = new FileStream(Path.Combine(directoryPath, renameFile), FileMode.Create))
                            await file.CopyToAsync(fileStream);

                    return Redirect(Request.Headers["referer"]);
                }
                else
                {
                    TempData["RefererUrl"] = Request.Headers["referer"].ToString();
                    TempData["Message"] = "Unsupported file format.";
                    TempData["Description"] = $"The thumbnail image which you are trying to upload is in {extension} format. Please upload your thumbnail in *.jpg image format only.";

                    return RedirectToAction("ImageFormatNotSupported", "ErrorsAndExceptions", "Subscriptions");

                }
            }
            else
            {
                return Redirect(Request.Headers["referer"]);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ListingOwnerPhoto(int? listingId, IFormFile file)
        {
            if (file != null && listingId != null)
            {
                string directoryPath = hostingEnvironment.WebRootPath + "\\FileManager\\ListingOwnerPhoto\\";

                var extension = Path.GetExtension(file.FileName);

                if (extension == ".jpg")
                {
                    var renameFile = listingId + extension;

                    if (file.Length > 0)
                        using (var fileStream = new FileStream(Path.Combine(directoryPath, renameFile), FileMode.Create))
                            await file.CopyToAsync(fileStream);

                    return Redirect(Request.Headers["referer"]);
                }
                else
                {
                    TempData["RefererUrl"] = Request.Headers["referer"].ToString();
                    TempData["Message"] = "Unsupported file format.";
                    TempData["Description"] = $"The thumbnail image which you are trying to upload is in {extension} format. Please upload your thumbnail in *.jpg image format only.";

                    return RedirectToAction("ImageFormatNotSupported", "ErrorsAndExceptions", "Subscriptions");

                }
            }
            else
            {
                return Redirect(Request.Headers["referer"]);
            }
        }
    }
}


