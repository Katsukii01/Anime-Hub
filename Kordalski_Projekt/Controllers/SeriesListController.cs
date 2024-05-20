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
using System.Xml.Linq;

namespace Kordalski_Projekt.Controllers
{
    public class SeriesListController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SeriesListController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: SeriesList
        public async Task<IActionResult> Index()
        {
            return View(await _context.Series.ToListAsync());
        }

        // GET: SeriesList/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var series = await _context.Series
                .Include(s => s.Episode) // Include episodes related to the series
                .Include(s => s.Ratings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (series == null)
            {
                return NotFound();
            }

            // Przypisz użytkowników do ocen
            foreach (var rate in series.Ratings)
            {
                rate.User = await _userManager.FindByIdAsync(rate.User_Id);
            }

            return View(series);
        }

        // GET: SeriesList/EditRating
        public async Task<IActionResult> EditRating(int? id)
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

        // POST: SeriesList/EditRating
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRating(int id, [Bind("Id,User_Id,Series_Id,Rate,Description")] Ratings ratings)
        {
            if (id != ratings.Id)
            {
                return NotFound();
            }


            var currentUserId = _userManager.GetUserId(User);
            ratings.User_Id = currentUserId;
             _context.Update(ratings);
             await _context.SaveChangesAsync();
            


            return RedirectToAction(nameof(Details), new { id = ratings.Series_Id });

        }


        // GET: SeriesList/AddRating
        public IActionResult AddRating(int seriesId)
        {
            ViewData["SeriesId"] = seriesId;
            return View();
        }

        // POST: SeriesList/AddRating
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRating([Bind("Id,User_Id,Series_Id,Rate,Description")] Ratings rating)
        {

                int maxRatingId = _context.Ratings.Max(r => (int?)r.Id) ?? 0;
                rating.Id = maxRatingId + 1;

                _context.Add(rating);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = rating.Series_Id });

        }

        [HttpPost, ActionName("DeleteRating")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRating(int id)
        {

            var ratings = await _context.Ratings.FindAsync(id);
            var seriesId = ratings.Series_Id;
            if (ratings != null)
            {
                _context.Ratings.Remove(ratings);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = seriesId });
        }

    }
}
