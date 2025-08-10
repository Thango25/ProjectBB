using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Item Type")]
        public string Type { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [StringLength(100)]
        public string Brand { get; set; } 

        [StringLength(50)]
        public string Color { get; set; } 

        [Display(Name = "Date Reported")]
        [DataType(DataType.Date)]
        public DateTime DateReported { get; set; } = DateTime.Now;

        [Display(Name = "Date Lost/Found")]
        [DataType(DataType.Date)]
        public DateTime? DateLostFound { get; set; } 

        [Required]
        [StringLength(200)]
        public string Location { get; set; } 

        [StringLength(255)]
        [Display(Name = "Photo")]
        public string PhotoPath { get; set; } 

        public bool IsClaimed { get; set; } = false; 

        public string UserId { get; set; }
    }
}