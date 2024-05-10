using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BOL.AUDITTRAIL;
using DAL.AUDIT;

namespace FRONTEND.Controllers
{
    public class UserHistoriesController : Controller
    {
        private readonly AuditDbContext _context;

        public UserHistoriesController(AuditDbContext context)
        {
            _context = context;
        }

        // GET: UserHistories
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserHistory.ToListAsync());
        }

        // GET: UserHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userHistory = await _context.UserHistory
                .FirstOrDefaultAsync(m => m.HistoryID == id);
            if (userHistory == null)
            {
                return NotFound();
            }

            return View(userHistory);
        }

        // GET: UserHistories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserHistories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HistoryID,UserGuid,Email,Mobile,IPAddress,VisitDate,VisitTime,UserAgent,ReferrerURL,VisitedURL")] UserHistory userHistory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userHistory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userHistory);
        }

        // GET: UserHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userHistory = await _context.UserHistory.FindAsync(id);
            if (userHistory == null)
            {
                return NotFound();
            }
            return View(userHistory);
        }

        // POST: UserHistories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HistoryID,UserGuid,Email,Mobile,IPAddress,VisitDate,VisitTime,UserAgent,ReferrerURL,VisitedURL")] UserHistory userHistory)
        {
            if (id != userHistory.HistoryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userHistory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserHistoryExists(userHistory.HistoryID))
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
            return View(userHistory);
        }

        // GET: UserHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userHistory = await _context.UserHistory
                .FirstOrDefaultAsync(m => m.HistoryID == id);
            if (userHistory == null)
            {
                return NotFound();
            }

            return View(userHistory);
        }

        // POST: UserHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userHistory = await _context.UserHistory.FindAsync(id);
            _context.UserHistory.Remove(userHistory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserHistoryExists(int id)
        {
            return _context.UserHistory.Any(e => e.HistoryID == id);
        }
    }
}
