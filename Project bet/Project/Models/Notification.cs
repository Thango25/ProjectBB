using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class Notification
    {
        public int Id { get; set; }

        // The ID of the user who will receive the notification
        [Required]
        public string RecipientId { get; set; }

        // The title of the notification (e.g., "New Item Claim")
        [Required]
        public string Title { get; set; }

        // The main message body of the notification
        [Required]
        public string Message { get; set; }

        // A link or action associated with the notification (optional)
        public string? ActionUrl { get; set; }

        // Timestamp for when the notification was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Is the notification read by the user?
        public bool IsRead { get; set; } = false;

        // Foreign key to the item being claimed
        public int? ItemId { get; set; }

        // Foreign key to the user who claimed the item
        public string? ClaimerId { get; set; }
    }
}