using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BOL.PLAN;
using DAL.BILLING;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using BAL.Services.Contracts;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Area("Subscriptions")]
    public class SubscriptionsController : Controller
    {
        private readonly BillingDbContext _context;
        private readonly IUserService _userService;

        public SubscriptionsController(BillingDbContext context, IUserService userService)
        {
            _context = context;
            this._userService = userService;
        }

        // GET: Subscriptions/Subscriptions
        public async Task<IActionResult> Index()
        {
            var billingDbContext = _context.Subscription.Include(s => s.Period).Include(s => s.Product);
            return View(await billingDbContext.ToListAsync());
        }

        // GET: Subscriptions/Subscriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscription = await _context.Subscription
                .Include(s => s.Period)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.SubscriptionID == id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(subscription);
        }

        // GET: Subscriptions/Subscriptions/Create
        public async Task<IActionResult> Create()
        {
            ViewData["PeriodID"] = await _context.Period.OrderBy(p => p.DurationInMonths).ToListAsync();
            ViewData["ProductID"] = await _context.Product.Where(p => p.ProductType == "Listing Plans").ToListAsync();

            // Shafi: Get listing id from session
            ViewBag.ListingID = HttpContext.Session.GetInt32("ListingID");
            // End:

            return View();
        }

        // POST: Subscriptions/Subscriptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubscriptionID,ListingID,OwnerGuid,IPAddress,StartDate,StartTime,ModifyDate,RazorpayOrderID,RazorpayPaymentID,RazorpaySignature,ProductID,PeriodID,PaymentMethod,PaymentStatus,OrderStatus,CouponCode,OrderAmount,AcceptedTermsConditions")] Subscription subscription)
        {
            // Shafi: Get listing id from session
            ViewBag.ListingID = HttpContext.Session.GetInt32("ListingID");
            // End:

            ViewData["ProductID"] = await _context.Product.Where(p => p.ProductType == "Listing Plans").ToListAsync();
            ViewData["PeriodID"] = await _context.Period.OrderBy(p => p.DurationInMonths).ToListAsync();

            // Shafi: Get UserGuid & IP Address
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string ownerGuid = user.Id;
            // End:

            // Shafi: Assign Time Zone to CreatedDate & Created Time
            string mobile = user.PhoneNumber;
            DateTime timeZoneDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            // End:

            // Shafi: Get listing id from session
            ViewBag.ListingID = HttpContext.Session.GetInt32("ListingID");
            // End:

            // Shafi: Assign values in background
            subscription.OwnerGuid = ownerGuid;
            subscription.IPAddress = remoteIpAddress;
            subscription.ListingID = HttpContext.Session.GetInt32("ListingID").Value;
            subscription.StartDate = timeZoneDate;
            subscription.StartTime = timeZoneDate;
            subscription.ModifyDate = timeZoneDate;

            // Shafi: Assign payment details
            subscription.RazorpayPaymentID = "";
            subscription.RazorpayOrderID = "";
            subscription.RazorpaySignature = "";
            // End:

            // End:

            

            if (ModelState.IsValid)
            {
                _context.Add(subscription);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(subscription);
        }

        // GET: Subscriptions/Subscriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscription = await _context.Subscription.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }
            ViewData["PeriodID"] = new SelectList(_context.Period, "PeriodID", "DurationInMonths", subscription.PeriodID);
            ViewData["ProductID"] = new SelectList(_context.Product, "ProductID", "Description", subscription.ProductID);
            return View(subscription);
        }

        // POST: Subscriptions/Subscriptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubscriptionID,ListingID,OwnerGuid,IPAddress,StartDate,StartTime,ModifyDate,RazorpayOrderID,RazorpayPaymentID,RazorpaySignature,ProductID,PeriodID,PaymentMethod,PaymentStatus,OrderStatus,CouponCode,OrderAmount,AcceptedTermsConditions")] Subscription subscription)
        {
            if (id != subscription.SubscriptionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subscription);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubscriptionExists(subscription.SubscriptionID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PeriodID"] = new SelectList(_context.Period, "PeriodID", "DurationInMonths", subscription.PeriodID);
            ViewData["ProductID"] = new SelectList(_context.Product, "ProductID", "Description", subscription.ProductID);
            return View(subscription);
        }

        // GET: Subscriptions/Subscriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscription = await _context.Subscription
                .Include(s => s.Period)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.SubscriptionID == id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(subscription);
        }

        // POST: Subscriptions/Subscriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subscription = await _context.Subscription.FindAsync(id);
            _context.Subscription.Remove(subscription);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubscriptionExists(int id)
        {
            return _context.Subscription.Any(e => e.SubscriptionID == id);
        }
    }
}
