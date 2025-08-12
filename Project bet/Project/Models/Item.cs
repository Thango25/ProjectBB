using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Project.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        public ItemType Type { get; set; } // ✅ Uses the global enum

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
        [Required(ErrorMessage = "Please upload an image")]
        public IFormFile? ImageFile { get; set; }
    }
}
