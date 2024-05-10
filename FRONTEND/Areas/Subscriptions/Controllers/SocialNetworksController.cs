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
using BAL.Audit;
using BAL.Listings;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
    [Authorize]
    public class SocialNetworksController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly IUserService _userService;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;

        public SocialNetworksController(ListingDbContext listingContext, IUserService userService, 
            IHistoryAudit audit, IListingManager listingManager)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.listingManager = listingManager;
            this.audit = audit;
        }

        
        // GET: Subscriptions/SocialNetworks/Create
        public IActionResult Create()
        {
            // Shafi: Get listing id from session
            ViewBag.ListingID = HttpContext.Session.GetInt32("ListingID");
            // End:

            return View();
        }

        // POST: Subscriptions/SocialNetworks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SocialNetworkID,ListingID,OwnerGuid,IPAddress,WhatsappGroupLink,Youtube,Facebook,Instagram,Linkedin,Pinterest,Telegram,Others,Others1")] SocialNetwork socialNetwork)
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
            socialNetwork.OwnerGuid = ownerGuid;
            socialNetwork.IPAddress = remoteIpAddress;
            socialNetwork.ListingID = HttpContext.Session.GetInt32("ListingID").Value;
            // End:

            if (ModelState.IsValid)
            {
                listingContext.Add(socialNetwork);
                await listingContext.SaveChangesAsync();
                return RedirectToAction("Create", "Certifications", "Subscriptions");
            }
            return View(socialNetwork);
        }

        // GET: Subscriptions/SocialNetworks/Edit/5
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

            var socialNetwork = await listingContext.SocialNetwork.FindAsync(id);
            if (socialNetwork == null)
            {
                return NotFound();
            }

            // Shafi: Verify record ownership
            var owner = await listingManager.SocialOwnerAsync(socialNetwork.SocialNetworkID, socialNetwork.ListingID, userGuid);
            if (owner == true)
            {
                return View(socialNetwork);
            }
            else
            {
                return NotFound();
            }
            // End:
        }

        // POST: Subscriptions/SocialNetworks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SocialNetworkID,ListingID,OwnerGuid,IPAddress,WhatsappGroupLink,Youtube,Facebook,Instagram,Linkedin,Pinterest,Telegram,Others,Others1")] SocialNetwork socialNetwork)
        {
            // Shafi: Get UserGuid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string userGuid = user.Id;
            // End:

            if (id != socialNetwork.SocialNetworkID)
            {
                return NotFound();
            }

            // Shafi: Verify ownership
            var owner = await listingManager.SocialOwnerAsync(socialNetwork.SocialNetworkID, socialNetwork.ListingID, userGuid);
            if (owner == true)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        listingContext.Update(socialNetwork);
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
                        string updatedUrl = this.HttpContext.Request.Headers["Host"] + "/Subscriptions/SocialNetworks/Edit/" + socialNetwork.SocialNetworkID;
                        string activity = "Updated social network with id " + socialNetwork.SocialNetworkID;

                        // Shafi: Get user in roles
                        IList<string> userInRoleName = await _userService.GetRolesByUser(user);
                        string roleName = userInRoleName.FirstOrDefault();
                        // End:

                        await audit.CreateListingLastUpdatedAsync(socialNetwork.SocialNetworkID, userGuid, email, mobile, ipAddress, roleName, "Social Network", updatedDate, updatedTime, updatedUrl, userAgent, activity);
                        // End:
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!SocialNetworkExists(socialNetwork.SocialNetworkID))
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

            return View(socialNetwork);
        }

        private bool SocialNetworkExists(int id)
        {
            return listingContext.SocialNetwork.Any(e => e.SocialNetworkID == id);
        }
    }
}
