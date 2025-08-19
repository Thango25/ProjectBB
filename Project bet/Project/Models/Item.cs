using Microsoft.AspNetCore.Http;
using Project.Models;
using Project.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class Item
    {
        public int Id { get; set; }

        // The ID of the user who posted this item
        public string PostedById { get; set; } = string.Empty;

        [Required(ErrorMessage = "Item name is required")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verification question is required")]
        public string VerificationQuestion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verification answer is required")]
        public string VerificationAnswer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        public ItemType Type { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Date Reported")]
        [DataType(DataType.Date)]
        public DateTime DateLost { get; set; }

        [Display(Name = "Photo Path")]
        public string PhotoPath { get; set; } = string.Empty;

        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
    }
}
