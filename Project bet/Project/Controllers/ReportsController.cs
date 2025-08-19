using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;

namespace Project.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: ReportsController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ReportsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ReportsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReportsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ReportsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ReportsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ReportsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReportsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        public async Task<IActionResult> FrequentlyReportedLostItems(DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.Today;
            var start = startDate ?? end.AddDays(-30);

            var frequentlyReportedItems = await _context.Items
                .Where(i => i.Type == ItemType.Lost && i.DateLost >= start && i.DateLost <= end)
                .Include(i => i.Category)
                .GroupBy(i => i.Category.Name)
                .Select(g => new ItemReport
                {
                    ItemCategory = g.Key,
                    ReportedCount = g.Count()
                })
                .OrderByDescending(r => r.ReportedCount)
                .ToListAsync();

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");
            ViewBag.ReportTitle = $"Frequently Reported Lost Items ({start.ToShortDateString()} - {end.ToShortDateString()})";

            return View(frequentlyReportedItems);
        }
    }
}
