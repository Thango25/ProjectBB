using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [PersonalData]
        public string Name { get; set; }

        [Required]
        [PersonalData]
        public string Surname { get; set; }

        // The PhoneNumber attribute is already part of IdentityUser, so you don't need to add it here.
        // It's a good practice to use a display attribute for better UI.
        [Display(Name = "Phone Number")]
        public override string PhoneNumber { get; set; }
    }
}