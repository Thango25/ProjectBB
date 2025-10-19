using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System.Diagnostics;

namespace ProjectBBB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Calculate Category Counts (for the carousel)
            // Counts lost and found items grouped by category, excluding claimed items.
            var itemCounts = _context.Items
                .AsNoTracking()
                .Where(i => !i.IsClaimed)
                .GroupBy(i => i.CategoryId)
                .Select(g => new Project.Models.CategoryCounts
                {
                    CategoryId = g.Key,
                    ItemsLostCount = g.Count(i => i.Type == ItemType.Lost),
                    ItemsFoundCount = g.Count(i => i.Type == ItemType.Found)
                })
                .ToDictionary(x => x.CategoryId, x => x);

            // 2. Calculate the main KPI Totals (Total, Total Lost, Total Found)

            // Total number of unclaimed items
            int totalItems = _context.Items.Count(i => !i.IsClaimed);

            // Combined query for Total Lost and Total Found for efficiency
            var totals = _context.Items
                .AsNoTracking()
                .Where(i => !i.IsClaimed)
                .GroupBy(i => 1) // Group by constant to perform one aggregation query on the whole set
                .Select(g => new
                {
                    TotalLost = g.Count(i => i.Type == ItemType.Lost),
                    TotalFound = g.Count(i => i.Type == ItemType.Found)
                })
                .FirstOrDefault();

            // 3. Fetch ALL Categories
            var categories = _context.Categories
                .AsNoTracking()
                .ToList();

            // 4. Populate the HomeViewModel with all collected data
            var model = new HomeViewModel
            {
                Categories = categories,

                // CRITICAL KPI POPULATION
                TotalItems = totalItems,
                TotalLostItems = totals?.TotalLost ?? 0,
                TotalFoundItems = totals?.TotalFound ?? 0,

                // Fetch Recent Lost Items (Top 4)
                RecentLostItems = _context.Items
                    .Include(i => i.Category)
                    .Where(i => i.Type == ItemType.Lost && !i.IsClaimed)
                    .OrderByDescending(i => i.DateLost)
                    .Take(4)
                    .ToList(),

                // Fetch Recent Found Items (Top 4)
                RecentFoundItems = _context.Items
                    .Include(i => i.Category)
                    .Where(i => i.Type == ItemType.Found && !i.IsClaimed)
                    .OrderByDescending(i => i.DateLost)
                    .Take(4)
                    .ToList()
            };

            // 5. Store the Category Counts Dictionary in ViewData for the carousel rendering
            ViewData["CategoryCounts"] = itemCounts;

            return View(model);
        }
        [Authorize]

        public IActionResult Privacy()

        {

            return View();

        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public IActionResult Error()

        {

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        }

    }
}

