 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Kordalski_Projekt.Data;
using Kordalski_Projekt.Models;

namespace Kordalski_Projekt.Controllers
{
    public class WatchListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WatchListController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WatchList/idusera
        public async Task<IActionResult> Index(string userId)
        {
            // Pobierz wszystkie elementy listy oglądanych dla danego użytkownika
            var watchList = await _context.WatchedList
                .Include(w => w.Episode)
                .Include(w => w.User)
                .Where(w => w.User_Id == userId) // Filtruj na podstawie identyfikatora użytkownika
                .ToListAsync();

            return View(watchList);
        }

        // POST: WatchList/RemoveWatchedList
        [HttpPost]
        public async Task<IActionResult> RemoveWatchedList(int watchedListId)
        {
            var watchedList = await _context.WatchedList.FindAsync(watchedListId);
            if (watchedList == null)
            {
                return NotFound();
            }

            _context.WatchedList.Remove(watchedList);
            await _context.SaveChangesAsync();

            return Ok(); // Zwraca status 200 (OK) po pomyślnym usunięciu elementu WatchedList
        }
    }
}
