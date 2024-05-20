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
    public class LikesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LikesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Likes
        public async Task<IActionResult> Index()
        {
            var likes = await _context.Likes.Include(l => l.Episode).ToListAsync();

            foreach (var like in likes)
            {
                like.User = await _userManager.FindByIdAsync(like.User_Id);
            }

            return View(likes);
        }

        // GET: Likes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var likes = await _context.Likes
                .Include(l => l.Episode)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (likes == null)
            {
                return NotFound();
            }
            likes.User = await _userManager.FindByIdAsync(likes.User_Id);
            return View(likes);
        }

        // GET: Likes/Create
        public IActionResult Create()
        {
            ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Name");
            ViewData["User_Id"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: Likes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IsLiked,User_Id,Episode_Id")] Likes likes)
        {
           // if (ModelState.IsValid)
           // {
                var lastId = _context.Likes.OrderByDescending(x => x.Id).FirstOrDefault()?.Id ?? 0;
                likes.Id = lastId + 1;

                _context.Add(likes);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
           // }
           // ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Link", likes.Episode_Id);
           // ViewData["User_Id"] = new SelectList(_context.Users, "Id", "Id", likes.User_Id);
           // return View(likes);
        }

        // GET: Likes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var likes = await _context.Likes.FindAsync(id);
            if (likes == null)
            {
                return NotFound();
            }
            ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Name", likes.Episode_Id);
            ViewData["User_Id"] = new SelectList(_context.Users, "Id", "UserName", likes.User_Id);
            return View(likes);
        }

        // POST: Likes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IsLiked,User_Id,Episode_Id")] Likes likes)
        {
            if (id != likes.Id)
            {
                return NotFound();
            }

           // if (ModelState.IsValid)
            //{
                try
                {
                    _context.Update(likes);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LikesExists(likes.Id))
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
           // ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Link", likes.Episode_Id);
            //ViewData["User_Id"] = new SelectList(_context.Users, "Id", "Id", likes.User_Id);
           // return View(likes);
        }

        // GET: Likes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var likes = await _context.Likes
                .Include(l => l.Episode)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (likes == null)
            {
                return NotFound();
            }
            likes.User = await _userManager.FindByIdAsync(likes.User_Id);
            return View(likes);
        }

        // POST: Likes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var likes = await _context.Likes.FindAsync(id);
            if (likes != null)
            {
                _context.Likes.Remove(likes);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LikesExists(int id)
        {
            return _context.Likes.Any(e => e.Id == id);
        }
    }
}
