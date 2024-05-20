using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kordalski_Projekt.Models;
using Kordalski_Projekt.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kordalski_Projekt.Controllers
{
    public class AnimeRankingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnimeRankingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Pobierz wszystkie serie z bazdy danych
            var series = await _context.Series.ToListAsync();

            // Utwórz listę, która będzie przechowywać sumy ocen dla każdej serii
            var seriesTotalRatings = new Dictionary<int, int>(); // Zmieniono na int

            // Iteruj przez każdą serię i zlicz sumę ocen
            foreach (var serie in series)
            {
                seriesTotalRatings[serie.Id] = await _context.Ratings
                    .Where(r => r.Series_Id == serie.Id)
                    .CountAsync();
            }

            // Oblicz średnią ocenę dla każdej serii
            var seriesAverageRatings = new Dictionary<int, double>();
            foreach (var kvp in seriesTotalRatings)
            {
                int seriesId = kvp.Key;
                int totalRatingsCount = kvp.Value;

                // Jeżeli liczba ocen wynosi 0, średnia ocena jest NaN
                seriesAverageRatings[seriesId] = totalRatingsCount == 0 ? double.NaN :
                    (double)_context.Ratings.Where(r => r.Series_Id == seriesId).Sum(r => r.Rate) / totalRatingsCount;
            }

            // Posortuj serie według średniej oceny malejąco
            var sortedSeries = seriesAverageRatings.OrderByDescending(pair => pair.Value).ToList();

            // Przekazuje listę serii z rankingiem do widoku wraz z liczbą ocen
            ViewBag.Ranking = sortedSeries;
            ViewBag.RatingsCount = seriesTotalRatings;

            return View(series);
        }

    }
}
