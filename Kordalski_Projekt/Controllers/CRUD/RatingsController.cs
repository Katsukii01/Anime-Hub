using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Kordalski_Projekt.Data;
using Kordalski_Projekt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Kordalski_Projekt.Controllers.CRUD
{
    [Authorize(Policy = "RequireRole1")]
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RatingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Ratings
        public async Task<IActionResult> Index()
        {
            var ratings = await _context.Ratings.Include(r => r.Series).ToListAsync();

            foreach (var rating in ratings)
            {
                rating.User = await _userManager.FindByIdAsync(rating.User_Id);
            }

            return View(ratings);
        }
        // GET: Ratings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ratings = await _context.Ratings
                .Include(r => r.Series)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ratings == null)
            {
                return NotFound();
            }
            ratings.User = await _userManager.FindByIdAsync(ratings.User_Id);
            return View(ratings);
        }

        // GET: Ratings/Create
        public IActionResult Create()
        {
            ViewData["Series_Id"] = new SelectList(_context.Series, "Id", "Title");
            ViewData["User_Id"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: Ratings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,User_Id,Series_Id,Rate,Description")] Ratings ratings)
        {
            // if (ModelState.IsValid)
            // {
            var lastId = _context.Ratings.OrderByDescending(x => x.Id).FirstOrDefault()?.Id ?? 0;
            ratings.Id = lastId + 1;

            _context.Add(ratings);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            // }
            // ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Link", ratings.Episode_Id);
            // ViewData["User_Id"] = new SelectList(_context.Users, "Id", "Id", ratings.User_Id);
            // return View(ratings);
        }

        // GET: Ratings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ratings = await _context.Ratings.FindAsync(id);
            if (ratings == null)
            {
                return NotFound();
            }
            ViewData["Series_Id"] = new SelectList(_context.Series, "Id", "Title", ratings.Series_Id);
            ViewData["User_Id"] = new SelectList(_context.Users, "Id", "UserName", ratings.User_Id);
            return View(ratings);
        }

        // POST: Ratings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,User_Id,Series_Id,Rate,Description")] Ratings ratings)
        {
            if (id != ratings.Id)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                _context.Update(ratings);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RatingsExists(ratings.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
            //}
            //ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Link", ratings.Episode_Id);
            //ViewData["User_Id"] = new SelectList(_context.Users, "Id", "Id", ratings.User_Id);
            //return View(ratings);
        }

        // GET: Ratings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ratings = await _context.Ratings
                .Include(r => r.Series)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ratings == null)
            {
                return NotFound();
            }
            ratings.User = await _userManager.FindByIdAsync(ratings.User_Id);
            return View(ratings);
        }

        // POST: Ratings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ratings = await _context.Ratings.FindAsync(id);
            if (ratings != null)
            {
                _context.Ratings.Remove(ratings);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RatingsExists(int id)
        {
            return _context.Ratings.Any(e => e.Id == id);
        }
    }
}
