// Controllers/NotificationController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Only allow authenticated users to access this
public class NotificationController : ControllerBase
{
    // These are the dependencies that need to be injected
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    // The constructor injects the dependencies
    public NotificationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("GetUnread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        // Here, the userId variable is properly initialized
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Now, the _context and userId variables are in scope and can be used.
        var notifications = await _context.Notifications
                                           .Where(n => n.RecipientId == userId && !n.IsRead)
                                           .OrderByDescending(n => n.CreatedAt)
                                           .ToListAsync();
        return Ok(notifications);
    }

    [HttpPost("MarkAsRead/{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = _userManager.GetUserId(User);
        var notification = await _context.Notifications.FindAsync(id);

        if (notification == null || notification.RecipientId != userId)
        {
            return NotFound();
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return NoContent(); // 204 No Content success
    }
}