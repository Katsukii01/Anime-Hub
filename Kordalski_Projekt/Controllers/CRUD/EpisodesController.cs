using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Kordalski_Projekt.Data;
using Kordalski_Projekt.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace Kordalski_Projekt.Controllers.CRUD
{
    [Authorize(Policy = "RequireRole1")]
    public class EpisodesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EpisodesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Episodes
        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var episodes = _context.Episode
                                   .Include(e => e.Series)
                                   .OrderBy(e => e.Id)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

            var totalEpisodes = _context.Episode.Count();
            var totalPages = (int)Math.Ceiling(totalEpisodes / (double)pageSize);
            var hasPreviousPage = page > 1;
            var hasNextPage = page < totalPages;

            ViewBag.PageIndex = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = hasPreviousPage;
            ViewBag.HasNextPage = hasNextPage;

            return View(episodes);
        }

        // GET: Episodes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var episode = await _context.Episode
                .Include(e => e.Series)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (episode == null)
            {
                return NotFound();
            }

            return View(episode);
        }

        // GET: Episodes/Create
        public IActionResult Create()
        {
            ViewData["Series_Id"] = new SelectList(_context.Series, "Id", "Title");
            return View();
        }

        // POST: Episodes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Series_Id,Name,Link,Prev_src")] Episode episode, [FromForm(Name = "photoInput")] Microsoft.AspNetCore.Http.IFormFile photoInput)
        {
            var maxId = await _context.Episode.MaxAsync(e => (int?)e.Id) ?? 0;
            episode.Id = maxId + 1;

            if (photoInput != null && photoInput.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "episodes");
                string uniqueFileName = episode.Id.ToString() + ".png";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photoInput.CopyToAsync(fileStream);
                }
                episode.Prev_src = "/episodes/" + uniqueFileName;
            }

            _context.Add(episode);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Episodes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var episode = await _context.Episode.FindAsync(id);
            if (episode == null)
            {
                return NotFound();
            }
            ViewData["Series_Id"] = new SelectList(_context.Series, "Id", "Title", episode.Series_Id);
            return View(episode);
        }

        // POST: Episodes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Series_Id,Name,Link,Prev_src")] Episode episode, [FromForm(Name = "photoInput")] Microsoft.AspNetCore.Http.IFormFile photoInput)
        {
            if (id != episode.Id)
            {
                return NotFound();
            }

            if (photoInput != null && photoInput.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "episodes");
                string uniqueFileName = episode.Id.ToString() + ".png";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photoInput.CopyToAsync(fileStream);
                }
                episode.Prev_src = "/episodes/" + uniqueFileName;
            }

            try
            {
                _context.Update(episode);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EpisodeExists(episode.Id))
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

        // GET: Episodes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var episode = await _context.Episode
                .Include(e => e.Series)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (episode == null)
            {
                return NotFound();
            }

            return View(episode);
        }

        // POST: Episodes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var episode = await _context.Episode.FindAsync(id);
            if (episode == null)
            {
                return NotFound();
            }
            // Usuń plik zdjęcia
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "episodes");
            string uniqueFileName = episode.Id.ToString() + ".png";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Usuń wszystkie powiązane obiekty przed usunięciem odcinka
            var likes = _context.Likes.Where(l => l.Episode_Id == id);
            _context.Likes.RemoveRange(likes);

            var comments = _context.Comments.Where(c => c.Episode_Id == id);
            _context.Comments.RemoveRange(comments);

            var ratings = _context.Ratings.Where(r => r.Series_Id == id);
            _context.Ratings.RemoveRange(ratings);

            var watchedLists = _context.WatchedList.Where(w => w.Episode_Id == id);
            _context.WatchedList.RemoveRange(watchedLists);

            _context.Episode.Remove(episode);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EpisodeExists(int id)
        {
            return _context.Episode.Any(e => e.Id == id);
        }
    }
}
