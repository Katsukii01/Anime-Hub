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
using Microsoft.AspNetCore.Authorization;

namespace Kordalski_Projekt.Controllers.CRUD
{
    [Authorize(Policy = "RequireRole1")]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            var comments = await _context.Comments.Include(c => c.Episode).ToListAsync();

            foreach (var comment in comments)
            {
                comment.User = await _userManager.FindByIdAsync(comment.User_Id);
            }

            return View(comments);
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Episode)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (comment == null)
            {
                return NotFound();
            }
     
             comment.User = await _userManager.FindByIdAsync(comment.User_Id);
            
            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Name");
            ViewData["User_Id"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,Time,User_Id,Episode_Id")] Comments comments)
        {
            //if (ModelState.IsValid)
            //{
                var lastId = _context.Comments.OrderByDescending(x => x.Id).FirstOrDefault()?.Id ?? 0;
                comments.Id = lastId + 1;

                _context.Add(comments);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
           // }
           // ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Link", comments.Episode_Id);
           // ViewData["User_Id"] = new SelectList(_context.Users, "Id", "Id", comments.User_Id);
           // return View(comments);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments.FindAsync(id);
            if (comments == null)
            {
                return NotFound();
            }
            ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Name", comments.Episode_Id);
            ViewData["User_Id"] = new SelectList(_context.Users, "Id", "UserName", comments.User_Id);
            return View(comments);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,Time,User_Id,Episode_Id")] Comments comments)
        {
            if (id != comments.Id)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
                try
                {
                    _context.Update(comments);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentsExists(comments.Id))
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
            //ViewData["Episode_Id"] = new SelectList(_context.Episode, "Id", "Link", comments.Episode_Id);
            //ViewData["User_Id"] = new SelectList(_context.Users, "Id", "Id", comments.User_Id);
           // return View(comments);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments
                .Include(c => c.Episode)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comments == null)
            {
                return NotFound();
            }
            comments.User = await _userManager.FindByIdAsync(comments.User_Id);
            return View(comments);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comments = await _context.Comments.FindAsync(id);
            if (comments != null)
            {
                _context.Comments.Remove(comments);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentsExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
