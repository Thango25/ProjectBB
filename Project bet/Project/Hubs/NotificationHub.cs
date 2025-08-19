using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Project.Hubs
{
    public class NotificationHub : Hub
    {
        // This method can be called from your controllers to send a notification to a specific user.
        // It requires the user's unique identifier (e.g., UserId).
        public async Task SendNotificationToUser(string userId, string title, string message)
        {
            // The 'User' in the following line is a built-in property of the Hub class
            // that maps a user ID to their active connections.
            await Clients.User(userId).SendAsync("ReceiveNotification", title, message);
        }

        // Example: a method to send a notification to all connected users.
        public async Task SendNotificationToAll(string title, string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", title, message);
        }
    }
}
