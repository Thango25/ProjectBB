using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System.Diagnostics;

namespace ProjectBBB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // ? Add DbContext


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

                // ? Get all lost items by removing .Take(4)
                RecentLostItems = _context.Items
                    .Where(i => i.Type == ItemType.Lost)
                    .OrderByDescending(i => i.DateLost)
                    .ToList(),

                // ? Get all found items by removing .Take(4)
                RecentFoundItems = _context.Items
                    .Where(i => i.Type == ItemType.Found)
                    .OrderByDescending(i => i.DateLost)
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