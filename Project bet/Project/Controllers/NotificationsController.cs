using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Project.Data;
using Project.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Project.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var notifications = _context.Notifications
                                        .Where(n => n.UserId == user.Id)
                                        .OrderByDescending(n => n.CreatedAt)
                                        .ToList();

            return View(notifications);
        }

        // ==========================================================
        // GET: /api/notifications/user
        // Fetch all notifications for the currently logged-in user
        // ==========================================================
        [HttpGet("user")]
        public async Task<IActionResult> GetUserNotifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    CreatedAt = n.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                    n.IsRead
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // ==========================================================
        // POST: /api/notifications/mark-read/{id}
        // Mark a specific notification as read
        // ==========================================================
        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // ==========================================================
        // DELETE: /api/notifications/clear
        // Clear all notifications for current user
        // ==========================================================
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearNotifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var userNotifications = _context.Notifications.Where(n => n.UserId == user.Id);
            _context.Notifications.RemoveRange(userNotifications);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "All notifications cleared." });
        }
    }
}
