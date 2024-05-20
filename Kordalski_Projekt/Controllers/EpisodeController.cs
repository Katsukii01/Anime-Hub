using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kordalski_Projekt.Data;
using Kordalski_Projekt.Models;

namespace Kordalski_Projekt.Controllers
{
    public class EpisodeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EpisodeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Episode/Watch/1
        public async Task<IActionResult> Watch(int? id, int page = 1, int pageSize = 10)
        {
            if (id == null)
            {
                return NotFound();
            }

            var episode = await _context.Episode
                .Include(e => e.Series)
                .FirstOrDefaultAsync(m => m.Id == id);


            // Check if the user is logged in
            if (User.Identity.IsAuthenticated)
            {
                var currentuId = _userManager.GetUserId(User);

                // Check if the current episode is already in the user's watched list
                var alreadyWatched = await _context.WatchedList
                    .AnyAsync(w => w.User_Id == currentuId && w.Episode_Id == id);

                if (!alreadyWatched)
                {
                    // Find the maximum watched list id
                    var maxWatchedListId = await _context.WatchedList
                        .MaxAsync(w => (int?)w.Id) ?? 0;

                    // Add a new record to the watched list
                    var newWatchedList = new WatchedList
                    {
                        Id = maxWatchedListId + 1,
                        User_Id = currentuId,
                        Episode_Id = id.Value
                    };

                    _context.WatchedList.Add(newWatchedList);
                    await _context.SaveChangesAsync();
                }
            }

            if (episode == null)
            {
                return NotFound();
            }

            // Pobierz komentarze dla tego odcinka, posortuj po dacie od najnowszych i zastosuj paginację
            var commentsQuery = _context.Comments
                .Where(c => c.Episode_Id == id)
                .OrderByDescending(c => c.Time);

            var totalComments = await commentsQuery.CountAsync();
            var comments = await commentsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Znajdź użytkowników na podstawie user_id z komentarzy i przypisz ich do komentarzy
            foreach (var comment in comments)
            {
                comment.User = await _userManager.FindByIdAsync(comment.User_Id);
            }

            // Pobierz sumę polubień i dyslajków dla tego odcinka
            var likesCount = await _context.Likes
                .Where(l => l.Episode_Id == id && l.IsLiked == true)
                .CountAsync();

            var dislikesCount = await _context.Likes
                .Where(l => l.Episode_Id == id && l.IsLiked == false)
                .CountAsync();

            var currentUserId = _userManager.GetUserId(User);
            var userLikeStatus = await _context.Likes
                .FirstOrDefaultAsync(l => l.Episode_Id == id && l.User_Id == currentUserId);

            ViewData["DislikesCount"] = dislikesCount;
            ViewData["UserLikeStatus"] = userLikeStatus;
            ViewData["Comments"] = comments;
            ViewData["LikesCount"] = likesCount;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalComments / (double)pageSize);

            var previousEpisode = await _context.Episode
                .Where(e => e.Id < id && e.Series_Id == episode.Series_Id)
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync();

            var nextEpisode = await _context.Episode
                .Where(e => e.Id > id && e.Series_Id == episode.Series_Id)
                .OrderBy(e => e.Id)
                .FirstOrDefaultAsync();

            ViewData["PreviousEpisode"] = previousEpisode;
            ViewData["NextEpisode"] = nextEpisode;

            return View(episode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, string userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (userId != currentUser.Id)
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(string commentText, int episodeId, string userId)
        {
            try
            {
                if (!string.IsNullOrEmpty(commentText))
                {
                    var lastId = _context.Comments.OrderByDescending(x => x.Id).FirstOrDefault()?.Id ?? 0;
                    var newComment = new Comments
                    {
                        Id = lastId + 1,
                        Content = commentText,
                        Time = DateTime.Now, // Set the time to the current time
                        User_Id = userId,
                        Episode_Id = episodeId
                    };

                    _context.Comments.Add(newComment);
                    await _context.SaveChangesAsync();

                    // Return the newly created comment as JSON
                    return Json(newComment);
                }

                // If comment text is empty or null, return a bad request response
                return BadRequest("Comment text cannot be empty.");
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                // You can use a logging framework like Serilog, NLog, or log it to the console
                // Logging ex.ToString() gives you detailed information about the exception
                Console.WriteLine("Exception occurred in AddComment action method:");
                Console.WriteLine(ex.ToString());

                // Return an error response with a generic error message
                return BadRequest("An error occurred while adding the comment. Please try again.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLikeDislike(int episodeId, bool isLiked)
        {
            var currentUserId = _userManager.GetUserId(User);
            var userLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.Episode_Id == episodeId && l.User_Id == currentUserId);
            string last  = null;

            if (userLike == null)
            {
                var lastId = _context.Likes.OrderByDescending(x => x.Id).FirstOrDefault()?.Id ?? 0;
                userLike = new Likes
                {
                    Id = lastId + 1,
                    Episode_Id = episodeId,
                    User_Id = currentUserId,
                    IsLiked = isLiked
                };
                _context.Likes.Add(userLike);
            }
            else
            {
                // Handle switching from like to dislike and vice versa
                if (userLike.IsLiked == isLiked)
                {
                    last = "niema";
                    // Remove the like/dislike if the value is the same
                    _context.Likes.Remove(userLike);
                }
                else
                {
                    last = "zmiana";
                    userLike.IsLiked = isLiked;
                    _context.Likes.Update(userLike);
                }
            }

            await _context.SaveChangesAsync();

            var likesCount = await _context.Likes
                .Where(l => l.Episode_Id == episodeId && l.IsLiked == true)
                .CountAsync();

            var dislikesCount = await _context.Likes
                .Where(l => l.Episode_Id == episodeId && l.IsLiked == false)
                .CountAsync();

            return Json(new { likesCount, dislikesCount, userLikeStatus = userLike?.IsLiked, status=last });
        }


    }
}
