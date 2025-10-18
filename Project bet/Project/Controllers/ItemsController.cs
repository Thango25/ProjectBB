using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Hubs;
using Project.Models;
using System.Text.Json;

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
            var items = await _context.Items.Include(i => i.Category).ToListAsync();
            return View(items);
        }

        // GET: Items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Items.Include(i => i.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        // --- PRIVATE HELPER FUNCTION: Saves notification to the database ---
        private async Task SaveNotificationToDb(string userId, string title, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // Search Action Method
        public async Task<IActionResult> Search(string type, int? categoryId)
        {
            var itemsQuery = _context.Items.Include(i => i.Category).AsQueryable();

            // Filter by Item Type if specified
            if (!string.IsNullOrEmpty(type))
            {
                if (Enum.TryParse(type, true, out ItemType itemType))
                {
                    itemsQuery = itemsQuery.Where(i => i.Type == itemType);
                }
            }

            // Filter by Category if specified
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                itemsQuery = itemsQuery.Where(i => i.CategoryId == categoryId.Value);
            }

            var items = await itemsQuery.ToListAsync();
            ViewData["Title"] = "Matching items";
            return View("DisplayMatchingItems", items);
        }

        // GET: Items/ByCategory
        public async Task<IActionResult> ByCategory(int categoryId, string itemType = null)
        {
            var itemsQuery = _context.Items
                .Include(i => i.Category)
                .Where(i => i.CategoryId == categoryId);

            // Filter by Item Type if specified
            if (!string.IsNullOrEmpty(itemType))
            {
                if (itemType.Equals("Lost", StringComparison.OrdinalIgnoreCase))
                {
                    itemsQuery = itemsQuery.Where(i => i.Type == ItemType.Lost);
                }
                else if (itemType.Equals("Found", StringComparison.OrdinalIgnoreCase))
                {
                    itemsQuery = itemsQuery.Where(i => i.Type == ItemType.Found);
                }
            }

            var items = await itemsQuery.ToListAsync();
            ViewData["Title"] = "Matching Items";
            return View("DisplayMatchingItems", items);
        }

        // --- ACTION: GET Verification Question (Used for Display Only) ---
        [HttpGet]
        public async Task<IActionResult> GetVerificationQuestion(int itemId)
        {
            var item = await _context.Items.FindAsync(itemId);

            if (item == null || string.IsNullOrEmpty(item.VerificationQuestion))
            {
                return Json(new { success = false, message = "Question not set." });
            }

            return Json(new { success = true, question = item.VerificationQuestion });
        }

        // --- MODIFIED ACTION: POST Send Claim Notification (Saves attempt) ---
        [HttpPost]
        public async Task<IActionResult> SendClaimNotification(int itemId, string ownerId, string answer)
        {
            var claimantId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(claimantId))
            {
                return Json(new { success = false, message = "You must be logged in to claim an item." });
            }

            var item = await _context.Items.FindAsync(itemId);
            string itemTitle = item?.Title ?? $"Item ID {itemId}";

            string verificationQuestion = item?.VerificationQuestion?.Trim() ?? "N/A";
            var storedAnswer = item?.VerificationAnswer?.Trim() ?? string.Empty;
            string verificationStatus = string.Equals(storedAnswer, answer.Trim(), StringComparison.OrdinalIgnoreCase)
                ? "VERIFIED (ANSWER MATCHES)."
                : "UNVERIFIED (ANSWER MISMATCHES).";

            const string title = "New Claim Attempt Received";

            // Create a detailed JSON payload that includes all necessary data for the UI
            var payload = new
            {
                NotificationType = "ClaimAttempt",
                Title = title,
                ItemTitle = itemTitle,
                ItemId = itemId,
                ClaimantId = claimantId,
                VerificationQuestion = verificationQuestion,
                ClaimantAnswer = answer,
                VerificationStatus = verificationStatus
            };

            // Serialize the payload into a string
            string jsonPayload = JsonSerializer.Serialize(payload);

            // 1. Save the detailed notification to the database for the item owner
            await SaveNotificationToDb(ownerId, title, jsonPayload);

            // 2. Send the FULL JSON Payload via SignalR.
            await _hubContext.Clients.User(ownerId).SendAsync("ReceiveDetailedNotification", title, jsonPayload);

            return Json(new
            {
                success = true,
                message = "Your claim details have been sent to the item poster."
            });
        }

        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int itemId, string claimantId)
        {
            var posterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.Items.FindAsync(itemId);

            if (item == null || item.PostedById != posterId)
            {
                return Json(new { success = false, message = "Item not found or unauthorized." });
            }

            var approver = await _userManager.GetUserAsync(User);
            var approverEmail = approver?.Email ?? "Unknown";

            // ✅ Set claim date when approving
            item.IsClaimed = true;
            item.ClaimDate = DateTime.Now;

            _context.Update(item);
            await _context.SaveChangesAsync();

            const string title = "Claim Approved! 🎉";
            string message = $"Title: {title}\nItem: {item.Title}\nApproved by: {approverEmail}";

            await SaveNotificationToDb(claimantId, title, message);

            await _hubContext.Clients.User(claimantId)
                .SendAsync("ReceiveNotification", title, message);

            return Json(new
            {
                success = true,
                message = $"Claim for '{item.Title}' approved. The claimant has been notified (Approver: {approverEmail})."
            });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteClaim(int itemId, string claimantId)
        {
            var posterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.Items.FindAsync(itemId);

            if (item == null || item.PostedById != posterId)
            {
                return Json(new { success = false, message = "Item not found or unauthorized." });
            }

            const string title = "Claim Rejected 😔";
            string message = $"The poster has declined your claim for the item '{item.Title}'. The item remains open for others to claim.";

            // 1. Save rejection notification to the database for the claimant
            await SaveNotificationToDb(claimantId, title, message);

            // 2. Notify the CLAIMANT of the rejection/deletion via SignalR (THIS LINE WAS MISSING/INCORRECTLY REPLACED)
            await _hubContext.Clients.User(claimantId).SendAsync("ReceiveNotification", title, message);

            // 3. Notify the POSTER of the successful action
            return Json(new { success = true, message = "Claim notification dismissed, and claimant has been notified of rejection." });
        }

        // Claim Item
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Claim(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var verifyUrl = Url.Action("Verify", "Items", new { id = item.Id }, Request.Scheme);

            await _hubContext.Clients.All.SendAsync(
                "ReceiveNotification",
                "Item Claimed",
                $"An item titled '{item.Title}' has been claimed.",
                verifyUrl
            );

            return Ok(new { success = true });
        }

        // Verify Claim
        [Authorize]
        public async Task<IActionResult> Verify(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            TempData["Message"] = $"Claim for '{item.Title}' has been verified.";
            return RedirectToAction("Details", new { id = id });
        }

        // Upload Lost Item
        [Authorize]
        public IActionResult UploadLostItem()
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
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
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");

            if (ModelState.IsValid)
            {
                item.Type = ItemType.Lost;
                item.PostedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

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
                else
                {
                    // Handle case where no image is uploaded (optional)
                    item.PhotoPath = "no-image.png";
                }

                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync(
                    "ReceiveNotification",
                    "New Lost Item Reported!",
                    $"A new lost item, {item.Title}, has been reported."
                );

                return RedirectToAction("Index");
            }
            return View("UploadItem", item);
        }

        // Upload Found Item
        [Authorize]
        public IActionResult UploadFoundItem()
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
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
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");

            if (ModelState.IsValid)
            {
                item.Type = ItemType.Found;
                item.PostedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

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
                else
                {
                    // Handle case where no image is uploaded (optional)
                    item.PhotoPath = "no-image.png";
                }

                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync(
                    "ReceiveNotification",
                    "New Found Item Reported!",
                    $"A new found item, {item.Title}, has been reported."
                );

                return RedirectToAction("Index");
            }
            return View("UploadItem", item);
        }

        // Edit Item
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item item)
        {
            if (id != item.Id) return NotFound();
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
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
        // Delete Item
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

        [HttpGet]
        public async Task<IActionResult> GetLatestItems()
        {
            var latestItems = await _context.Items
                                            .Include(i => i.Category)
                                            .OrderByDescending(i => i.DateLost)
                                            .Take(5)
                                            .Select(i => new
                                            {
                                                i.Id,
                                                i.Title,
                                                i.Description,
                                                i.Location,
                                                i.DateLost,
                                                CategoryName = i.Category.Name,
                                                i.PhotoPath,
                                                i.Type
                                            })
                                            .ToListAsync();
            return Json(latestItems);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryItemCounts()
        {
            var categoryItemCounts = await _context.Items
                                                   .Include(i => i.Category)
                                                   .GroupBy(i => i.Category.Name)
                                                   .Select(g => new
                                                   {
                                                       CategoryName = g.Key,
                                                       ItemCount = g.Count()
                                                   })
                                                   .OrderBy(g => g.CategoryName)
                                                   .ToListAsync();
            return Json(categoryItemCounts);
        }
    }
}