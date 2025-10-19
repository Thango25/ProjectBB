using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace Project.Models
{
    public class Profile : IdentityUser
    {
        // These are the only new properties you need to add.
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Location { get; set; }
    }
}
