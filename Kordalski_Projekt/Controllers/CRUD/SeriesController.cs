using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kordalski_Projekt.Data;
using Kordalski_Projekt.Models;
using Microsoft.AspNetCore.Authorization;

namespace Kordalski_Projekt.Controllers.CRUD
{
    [Authorize(Policy = "RequireRole1")]
    public class SeriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SeriesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Series
        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var totalSeries = _context.Series.Count();
            var totalPages = (int)Math.Ceiling(totalSeries / (double)pageSize);
            var series = _context.Series
                                   .OrderBy(e => e.Id)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

            ViewBag.PageIndex = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            return View(series);
        }


        // GET: Series/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var series = await _context.Series
                .FirstOrDefaultAsync(m => m.Id == id);
            if (series == null)
            {
                return NotFound();
            }

            return View(series);
        }

        // GET: Series/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST: Series/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Status,FirstAirDate,Photo_src")] Series series, [FromForm(Name = "photoInput")] Microsoft.AspNetCore.Http.IFormFile photoInput)
        {
          //  if (ModelState.IsValid)
            //{
                // Pobierz najwyższy istniejący Id w tabeli Series
                var maxId = await _context.Series.MaxAsync(s => (int?)s.Id) ?? 0;

                // Ustaw Id nowego rekordu na wartość o jeden większą
                series.Id = maxId + 1;

                _context.Add(series);
                await _context.SaveChangesAsync();

                if (photoInput != null && photoInput.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "series");
                    string uniqueFileName = series.Id.ToString() + ".png";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await photoInput.CopyToAsync(fileStream);
                    }
                    series.Photo_src = "/series/" + uniqueFileName;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
           // }
           // return View(series);
        }

        // GET: Series/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var series = await _context.Series.FindAsync(id);
            if (series == null)
            {
                return NotFound();
            }
            return View(series);
        }

        // POST: Series/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,FirstAirDate,Photo_src")] Series series, [FromForm(Name = "photoInput")] Microsoft.AspNetCore.Http.IFormFile photoInput)
        {
            if (id != series.Id)
            {
                return NotFound();
            }

           // if (ModelState.IsValid)
           // {
                try
                {
                    _context.Update(series);
                    await _context.SaveChangesAsync();

                    if (photoInput != null && photoInput.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "series");
                        string uniqueFileName = series.Id.ToString() + ".png";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await photoInput.CopyToAsync(fileStream);
                        }
                        series.Photo_src = "/series/" + uniqueFileName;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeriesExists(series.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
           // }
           // return View(series);
        }

        // GET: Series/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var series = await _context.Series
                .FirstOrDefaultAsync(m => m.Id == id);
            if (series == null)
            {
                return NotFound();
            }

            return View(series);
        }

        // POST: Series/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var series = await _context.Series
                .Include(s => s.Episode) // Wczytaj wszystkie epizody związane z serią
                 .FirstOrDefaultAsync(m => m.Id == id);
            _context.Episode.RemoveRange(series.Episode);

            // Usuń plik zdjęcia
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "series");
            string uniqueFileName = series.Id.ToString() + ".png";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.Series.Remove(series);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SeriesExists(int id)
        {
            return _context.Series.Any(e => e.Id == id);
        }

    }
}
