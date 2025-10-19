using Microsoft.AspNetCore.Identity;

namespace Project.Models // This namespace is crucial
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Location { get; set; }
    }
}