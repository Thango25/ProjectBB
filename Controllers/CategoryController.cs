using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;


namespace Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory; // Declare HttpClientFactory

        public CategoryController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory; // Initialize it in the constructor
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        public IActionResult Create()
        {
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IconClass")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IconClass")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //=========================================================================

        // BET MODIFIED: Added int? limit parameter

        // =========================================================================

        public async Task<IActionResult> Report(int? categoryId, DateTime? startDate, DateTime? endDate, int? limit)

        {

            var viewModel = new ReportsViewModel();

            var httpClient = _httpClientFactory.CreateClient();



            // Determine the item limit, defaulting to 5 if not provided or invalid

            int itemLimit = limit.HasValue && limit.Value >= 1 && limit.Value <= 20 ? limit.Value : 5;



            // 1. Fetch ALL Categories for the Filter Dropdown

            ViewBag.AvailableCategories = await _context.Categories.ToListAsync();



            // 2. Pass Filter Values back to the View for Persistence

            ViewBag.CurrentCategoryId = categoryId;

            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");

            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");

            ViewBag.CurrentLimit = itemLimit; // <--- NEW: Persist the limit



            // 3. Build the Query String for the API Calls

            var queryString = "";

            var filters = new List<string>();



            if (categoryId.HasValue)

            {

                filters.Add($"categoryId={categoryId.Value}");

            }

            if (startDate.HasValue)

            {

                filters.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("yyyy-MM-dd"))}");

            }

            if (endDate.HasValue)

            {

                filters.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("yyyy-MM-dd"))}");

            }

            // <--- NEW: Add the calculated limit to the query string

            filters.Add($"limit={itemLimit}");



            if (filters.Any())

            {

                queryString = "?" + string.Join("&", filters);

            }



            // 4. Fetch Latest Items (Filtered) from the ItemsController API

            var latestItemsResponse = await httpClient.GetAsync($"https://localhost:44382/Items/GetLatestItems{queryString}");

            // ... (rest of API call logic remains the same) ...

            if (latestItemsResponse.IsSuccessStatusCode)

            {

                var jsonString = await latestItemsResponse.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                viewModel.NewlyAddedItems = JsonSerializer.Deserialize<List<LatestItem>>(jsonString, options) ?? new List<LatestItem>();

            }



            // 5. Fetch Category Item Counts (No need to pass limit for counts)

            var categoryCountsResponse = await httpClient.GetAsync($"https://localhost:44382/Items/GetCategoryItemCounts{queryString}");

            // ... (rest of API call logic remains the same) ...



            return View(viewModel);

        }

        // =========================================================================

        // BET MODIFIED END 18/10/2025

        // =========================================================================


        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
