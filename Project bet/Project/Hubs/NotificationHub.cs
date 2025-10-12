// NotificationHub.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Project.Data;
using Project.Models; // Ensure you have this using statement
using System.Threading.Tasks;

namespace Project.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // <--- Add this

        public NotificationHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SendNotificationToUser(string ownerId, string title, string message)
        {
            if (string.IsNullOrEmpty(ownerId))
                throw new HubException("Owner ID is required.");

            var notification = new Notification
            {
                RecipientId = ownerId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await Clients.User(ownerId).SendAsync(
                "ReceiveNotification",
                notification.Title,
                notification.Message,
                notification.Id,
                notification.ItemId,
                notification.ClaimerId
            );
        }

        // 🎲 Claim-specific notifications
        public async Task SendClaimNotificationToOwner(string ownerId, int itemId, string claimerId)
        {
            if (string.IsNullOrEmpty(ownerId))
                throw new HubException("Owner ID is required.");
            if (string.IsNullOrEmpty(claimerId))
                throw new HubException("Claimer ID is required.");

            var notification = new Notification
            {
                RecipientId = ownerId,
                Title = "Item Claimed",
                Message = $"Your item with ID {itemId} was claimed by user {claimerId}.",
                ItemId = itemId,
                ClaimerId = claimerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await Clients.User(ownerId).SendAsync(
                "ReceiveNotification",
                notification.Title,
                notification.Message,
                notification.Id,
                notification.ItemId,
                notification.ClaimerId
            );
        }

        // New method to handle an approved claim
        public async Task ApproveClaim(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return;

            // Mark the original notification as read/handled
            notification.IsRead = true;
            await _context.SaveChangesAsync();

            // Find the owner of the item
            var ownerId = notification.RecipientId;
            var claimerId = notification.ClaimerId;

            // Create a new approval notification for the claimer
            var approvalNotification = new Notification
            {
                RecipientId = claimerId,
                Title = "Claim Approved",
                Message = "Your claim for an item has been approved!",
                ItemId = notification.ItemId
            };

            _context.Notifications.Add(approvalNotification);
            await _context.SaveChangesAsync();

            // Send the real-time approval notification to the claimer
            await Clients.User(claimerId).SendAsync("ReceiveNotification", approvalNotification.Title, approvalNotification.Message, approvalNotification.Id);
        }

        // New method to handle a declined claim
        public async Task DeclineClaim(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return;

            // Mark the original notification as read/handled
            notification.IsRead = true;
            await _context.SaveChangesAsync();

            var claimerId = notification.ClaimerId;

            // Create a new decline notification for the claimer
            var declineNotification = new Notification
            {
                RecipientId = claimerId,
                Title = "Claim Declined",
                Message = "Your claim for an item has been declined.",
                ItemId = notification.ItemId
            };

            _context.Notifications.Add(declineNotification);
            await _context.SaveChangesAsync();

            // Send the real-time decline notification to the claimer
            await Clients.User(claimerId).SendAsync("ReceiveNotification", declineNotification.Title, declineNotification.Message, declineNotification.Id);
        }
    }
}