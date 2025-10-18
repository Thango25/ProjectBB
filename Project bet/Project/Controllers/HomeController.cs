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
            var model = new HomeViewModel
            {
                Categories = _context.Categories.ToList(),

                RecentLostItems = _context.Items
                    .Where(i => i.Type == ItemType.Lost && !i.IsClaimed)
                    .OrderByDescending(i => i.DateLost)
                    .Take(4)
                    .ToList(),

                RecentFoundItems = _context.Items
                    .Where(i => i.Type == ItemType.Found && !i.IsClaimed)
                    .OrderByDescending(i => i.DateLost)
                    .Take(4)
                    .ToList()
            };

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
