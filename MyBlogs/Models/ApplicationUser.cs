using Microsoft.AspNetCore.Identity;

namespace MyBlogs.Models
{
    // This adds the "Name" column to your database
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}