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
    public class WatchedListsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public WatchedListsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: WatchedLists
        public async Task<IActionResult> Index()
        {
            var watchedLists = await _context.WatchedList.Include(w => w.Episode).ToListAsync();

            foreach (var watchedList in watchedLists)
            {
                watchedList.User = await _userManager.FindByIdAsync(watchedList.User_Id);
            }

            return View(watchedLists);
        }

        // GET: WatchedLists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var watchedList = await _context.WatchedList
                .Include(w => w.Episode)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (watchedList == null)
            {
                return NotFound();
            }
            watchedList.User = await _userManager.FindByIdAsync(watchedList.User_Id);
            return View(watchedList);
        }

        // GET: WatchedLists/Create
        public IActionResult Create()
        {
            ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Name");
            ViewData["User_Id"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: WatchedLists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,User_Id,Episode_Id")] WatchedList watchedList)
        {
            //if (ModelState.IsValid)
           // {
                var lastId = _context.WatchedList.OrderByDescending(x => x.Id).FirstOrDefault()?.Id ?? 0;
                watchedList.Id = lastId + 1;

                _context.Add(watchedList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
           // }
           // ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Link", watchedList.Episode_Id);
            //ViewData["User_Id"] = new SelectList(_context.Users, "Id", "Id", watchedList.User_Id);
            //return View(watchedList);
        }

        // GET: WatchedLists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var watchedList = await _context.WatchedList.FindAsync(id);
            if (watchedList == null)
            {
                return NotFound();
            }
            ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Name", watchedList.Episode_Id);
            ViewData["User_Id"] = new SelectList(_context.Users, "Id", "UserName", watchedList.User_Id);
            return View(watchedList);
        }

        // POST: WatchedLists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,User_Id,Episode_Id")] WatchedList watchedList)
        {
            if (id != watchedList.Id)
            {
                return NotFound();
            }

           // if (ModelState.IsValid)
           // {
               try
                {
                    _context.Update(watchedList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WatchedListExists(watchedList.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
               }
                //return RedirectToAction(nameof(Index));
            //}
            ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Name", watchedList.Episode_Id);
            ViewData["User_Id"] = new SelectList(_context.Users, "Id", "UserName", watchedList.User_Id);
            return RedirectToAction(nameof(Index));
        }

        // GET: WatchedLists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var watchedList = await _context.WatchedList
                .Include(w => w.Episode)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (watchedList == null)
            {
                return NotFound();
            }
            watchedList.User = await _userManager.FindByIdAsync(watchedList.User_Id);
            return View(watchedList);
        }

        // POST: WatchedLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var watchedList = await _context.WatchedList.FindAsync(id);
            if (watchedList != null)
            {
                _context.WatchedList.Remove(watchedList);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WatchedListExists(int id)
        {
            return _context.WatchedList.Any(e => e.Id == id);
        }
    }
}
