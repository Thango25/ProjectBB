using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Hubs;
using Project.Models;


namespace Project.Controllers
    {
        public class ItemsController : Controller
        {
            private readonly ApplicationDbContext _context;
            private readonly IWebHostEnvironment _webHostEnvironment;
            private readonly IHubContext<NotificationHub> _hubContext;
            private readonly UserManager<ApplicationUser> _userManager;

            public ItemsController(
                ApplicationDbContext context,
                IWebHostEnvironment webHostEnvironment,
                IHubContext<NotificationHub> hubContext,
                UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _webHostEnvironment = webHostEnvironment;
                _hubContext = hubContext;
                _userManager = userManager;
            }

            // GET: Items
            [Authorize]
            public async Task<IActionResult> Index()
            {
                return View(await _context.Items.ToListAsync());
            }

            // GET: Items/Details/5
            public async Task<IActionResult> Details(int? id)
            {
                if (id == null) return NotFound();

            // Include Category to eager load related data if needed
            var item = await _context.Items.Include(i => i.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();

                return View(item);
            }
        // In your ItemsController.cs

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Claim(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            // Get the ID of the user who posted the item
            var ownerId = item.PostedById;

            if (!string.IsNullOrEmpty(ownerId))
            {
                // 🚨 Corrected code: Use Clients.User() to target the owner 🚨
                await _hubContext.Clients.User(ownerId).SendAsync(
                    "ReceiveNotification",
                    "Item Claimed",
                    $"An item titled '{item.Title}' has been claimed.",
                    Url.Action("Verify", "Items", new { id = item.Id }, Request.Scheme)
                );
            }
            else
            {
                // Optional: Handle the case where the item has no owner ID
                // Log an error or return a message
                Console.WriteLine($"Error: Item with ID {id} has no owner specified.");
            }

            // It's a good idea to add a database entry for the notification here
            // so it's not lost if the user is offline.
            var notification = new Notification
            {
                RecipientId = ownerId,
                Message = $"Your item '{item.Title}' has been claimed!",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // -------------------------------
        // 📌 Verify Claim
        // -------------------------------
        [Authorize]
            public async Task<IActionResult> Verify(int id)
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null) return NotFound();

                // At this point, you could mark the item as 'claimed' or 'pending verification' in the database.
                // Example:
                // item.Status = ItemStatus.Claimed; 
                // await _context.SaveChangesAsync();

                TempData["Message"] = $"Claim for '{item.Title}' has been verified.";

                return RedirectToAction("Details", new { id = id });
            }

            // -------------------------------
            // Upload Lost Item
            // -------------------------------
            public IActionResult UploadLostItem()
            {
                var item = new Item
                {
                    Type = ItemType.Lost,
                    DateLost = DateTime.Today
                };
                return View("UploadItem", item);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            [Authorize]
            public async Task<IActionResult> UploadLostItem(Item item)
            {
                if (ModelState.IsValid)
                {
                    item.Type = ItemType.Lost;
                    item.PostedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    // Handle image upload
                    if (item.ImageFile != null && item.ImageFile.Length > 0)
                    {
                        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadDir))
                            Directory.CreateDirectory(uploadDir);

                        string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(item.ImageFile.FileName);
                        string filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await item.ImageFile.CopyToAsync(stream);
                        }

                        item.PhotoPath = fileName;
                    }

                    _context.Items.Add(item);
                    await _context.SaveChangesAsync();

                    // Notify all clients via SignalR
                    await _hubContext.Clients.All.SendAsync(
                        "ReceiveNotification",
                        "New Lost Item Reported!",
                        $"A new lost item, {item.Title}, has been reported."
                    );

                    return RedirectToAction("Index");
                }

                return View("UploadItem", item);
            }


            // -------------------------------
            // Upload Found Item
            // -------------------------------
            [Authorize]
            public IActionResult UploadFoundItem()
            {
                var item = new Item
                {
                    Type = ItemType.Found,
                    DateLost = DateTime.Today
                };
                return View("UploadItem", item);
            }


            [HttpPost]
            [ValidateAntiForgeryToken]
            [Authorize]
            public async Task<IActionResult> UploadFoundItem(Item item)
            {
                if (ModelState.IsValid)
                {
                    item.Type = ItemType.Found;
                    item.PostedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    // Handle image upload
                    if (item.ImageFile != null && item.ImageFile.Length > 0)
                    {
                        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadDir))
                            Directory.CreateDirectory(uploadDir);

                        string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(item.ImageFile.FileName);
                        string filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await item.ImageFile.CopyToAsync(stream);
                        }

                        item.PhotoPath = fileName;
                    }

                    _context.Items.Add(item);
                    await _context.SaveChangesAsync();

                    // Notify all clients via SignalR
                    await _hubContext.Clients.All.SendAsync(
                        "ReceiveNotification",
                        "New Found Item Reported!",
                        $"A new lost item, {item.Title}, has been reported."
                    );

                    return RedirectToAction("Index");
                }

                return View("UploadItem", item);
            }


            // -------------------------------
            // Edit Item
            // -------------------------------
            public async Task<IActionResult> Edit(int? id)
            {
                if (id == null) return NotFound();

                var item = await _context.Items.FindAsync(id);
                if (item == null) return NotFound();

                return View(item);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Item item)
            {
                if (id != item.Id) return NotFound();

                if (ModelState.IsValid)
                {
                    try
                    {
                        var existingItem = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                        if (existingItem == null) return NotFound();

                        if (item.ImageFile != null)
                        {
                            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(item.ImageFile.FileName);
                            string filePath = Path.Combine(uploadDir, fileName);

                            if (!Directory.Exists(uploadDir))
                                Directory.CreateDirectory(uploadDir);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await item.ImageFile.CopyToAsync(stream);
                            }

                            item.PhotoPath = fileName;
                        }
                        else
                        {
                            item.PhotoPath = existingItem.PhotoPath;
                        }

                        _context.Update(item);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ItemExists(item.Id)) return NotFound();
                        else throw;
                    }
                    return RedirectToAction(nameof(Index));
                }

                return View(item);
            }

            // -------------------------------
            // Delete Item
            // -------------------------------
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null) return NotFound();

                var item = await _context.Items.FirstOrDefaultAsync(m => m.Id == id);
                if (item == null) return NotFound();

                return View(item);
            }


            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var item = await _context.Items.FindAsync(id);
                if (item != null)
                {
                    _context.Items.Remove(item);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            private bool ItemExists(int id)
            {
                return _context.Items.Any(e => e.Id == id);
            }

        /// <summary>
        /// Retrieves the 5 most recently reported items.
        /// </summary>
        /// <returns>A JSON array of the latest 5 items.</returns>
        [HttpGet]
        public async Task<IActionResult> GetLatestItems()
        {
            var latestItems = await _context.Items
                                            .Include(i => i.Category) // Include Category for report
                                            .OrderByDescending(i => i.DateLost) // Assuming DateLost is the reporting date
                                            .Take(5)
                                            .Select(i => new
                                            {
                                                i.Id,
                                                i.Title,
                                                i.Description,
                                                i.Location,
                                                i.DateLost,
                                                CategoryName = i.Category.Name, // Include category name
                                                i.PhotoPath,
                                                i.Type // Lost/Found type
                                            })
                                            .ToListAsync();

            return Json(latestItems);
        }

        /// <summary>
        /// Retrieves the total number of items in each category.
        /// </summary>
        /// <returns>A JSON array of category names and their respective item counts.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCategoryItemCounts()
        {
            var categoryItemCounts = await _context.Items
                                                    .Include(i => i.Category) // Ensure Category is loaded for grouping
                                                    .GroupBy(i => i.Category.Name)
                                                    .Select(g => new
                                                    {
                                                        CategoryName = g.Key,
                                                        ItemCount = g.Count()
                                                    })
                                                    .OrderBy(g => g.CategoryName) // Order by category name for consistency
                                                    .ToListAsync();

            return Json(categoryItemCounts);
        }
    }
}



  