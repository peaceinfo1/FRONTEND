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
using BAL.Services.Contracts;

namespace FRONTEND.Areas.Analytics.Controllers
{
    [Area("Analytics")]
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly AuditDbContext auditContext;
        private readonly IUserService _userService;

        public HistoryController(AuditDbContext auditContext, IUserService userService)
        {
            this.auditContext = auditContext;
            this._userService = userService;
        }

        // GET: Analytics/History
        public async Task<IActionResult> Index()
        {
            // Shafi: Get user guid
            var user = await _userService.GetUserByUserName(User.Identity.Name);
            string UserGuid = user.Id;
            // End:

            return View(await auditContext.UserHistory.Where(h => h.UserGuid == UserGuid).OrderByDescending(h => h.HistoryID).ToListAsync());
        }
    }
}
