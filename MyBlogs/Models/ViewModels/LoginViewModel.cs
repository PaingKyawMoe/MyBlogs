using System.ComponentModel.DataAnnotations;

namespace MyBlogs.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "The Email is required")]
        [EmailAddress(ErrorMessage = "The Email is not valid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
