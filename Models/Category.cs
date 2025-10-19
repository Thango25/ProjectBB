using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Icon class is required")]
        [Display(Name = "Font Awesome Icon Class")]
        public string IconClass { get; set; }
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
