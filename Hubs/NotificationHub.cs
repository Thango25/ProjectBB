using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System;
using System.Threading.Tasks;

namespace Project.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public NotificationHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendNotificationToUser(string userId, string title, string message)
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

            await Clients.User(userId).SendAsync("ReceiveNotification", title, message);
        }

        // NEW: Method to notify the claimant their claim was approved
        public async Task SendClaimApprovedNotification(string claimantId, string itemTitle)
        {
            const string title = "Claim Approved! 🎉";
            var message = $"The poster has **approved your claim** for the item '{itemTitle}'. Please check your contact details in your profile for a message from the poster!";

            var notification = new Notification
            {
                UserId = claimantId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await Clients.User(claimantId).SendAsync("ReceiveNotification", title, message);
        }
    }
}