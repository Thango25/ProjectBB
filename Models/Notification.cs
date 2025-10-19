using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }   // Recipient of the notification

        [Required]
        public string Title { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        // Optional navigation property (if you want to link to IdentityUser)
        public ApplicationUser User { get; set; }
    }
}
