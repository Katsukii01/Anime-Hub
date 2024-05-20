using Kordalski_Projekt.Data;
using Kordalski_Projekt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Kordalski_Projekt.Controllers
{
    [Authorize(Policy = "RequireRole1")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UsersController(UserManager<IdentityUser> userManager, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;


        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            // Fetch all users
            var users = await _userManager.Users.ToListAsync();

            // Fetch users with the role ID 1
            var userIdsWithRoleId1 = await _context.UserRoles
                .Where(ur => ur.RoleId == "1") // Assuming RoleId is string type
                .Select(ur => ur.UserId)
                .ToListAsync();

            // Exclude users with RoleId 1
            var filteredUsers = users.Where(u => !userIdsWithRoleId1.Contains(u.Id)).ToList();

            return View(filteredUsers);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,UserName,Password")] CreateUserModel model, [FromForm(Name = "photoInput")] Microsoft.AspNetCore.Http.IFormFile photoInput)
        {
            Console.WriteLine($"Dane z formularza: Email: {model.Email}, UserName: {model.UserName}, Password: {model.Password}");

  
                var user = new IdentityUser { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
  
                    // Ustawienie hasła dla nowo utworzonego użytkownika
                    await _userManager.AddPasswordAsync(user, model.Password);

                    // Ustawienie stanu konta na aktywny
                    user.EmailConfirmed = true;

                    await _userManager.UpdateAsync(user);
                    if (photoInput != null && photoInput.Length > 0)
                    {
                        Console.WriteLine("Dodaje zdjęcie");
                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "pfp");
                        string uniqueFileName = user.Id.ToString() + ".png";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
   
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await photoInput.CopyToAsync(fileStream);
                            }
                    }
                    else
                    {
                        Console.WriteLine("Puste zdjęcie ");
                    }

                    Console.WriteLine("Użytkownik został pomyślnie utworzony.");

                    return RedirectToAction(nameof(Index));


        }


        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Email,UserName,PasswordHash")] IdentityUser model, [FromForm(Name = "photoInput")] Microsoft.AspNetCore.Http.IFormFile photoInput)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

           // if (ModelState.IsValid)
            //{
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.UserName;

                if (!string.IsNullOrEmpty(model.PasswordHash))
                {
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.PasswordHash);
                }

                var result = await _userManager.UpdateAsync(user);

                    if (photoInput != null && photoInput.Length > 0)
                    {
                        Console.WriteLine("Dodaje zdjęcie");
                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "pfp");
                        string uniqueFileName = user.Id.ToString() + ".png";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await photoInput.CopyToAsync(fileStream);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Puste zdjęcie ");
                    }
            // if (result.Succeeded)
            // {
            return RedirectToAction(nameof(Index));
               // }

               // foreach (var error in result.Errors)
               // {
                //    ModelState.AddModelError(string.Empty, error.Description);
               // }
            //}
           // return View(model);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Find and remove related objects before deleting the user
            var likes = _context.Likes.Where(l => l.User_Id == id);
            _context.Likes.RemoveRange(likes);

            var comments = _context.Comments.Where(c => c.User_Id == id);
            _context.Comments.RemoveRange(comments);

            var watchedLists = _context.WatchedList.Where(w => w.User_Id == id);
            _context.WatchedList.RemoveRange(watchedLists);

            var ratings = _context.Ratings.Where(r => r.User_Id == id);
            _context.Ratings.RemoveRange(ratings);

            await _context.SaveChangesAsync();

            // Usuń plik zdjęcia
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "pfp");
            string uniqueFileName = user.Id.ToString() + ".png";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Delete the user
            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
    public class CreateUserModel
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
