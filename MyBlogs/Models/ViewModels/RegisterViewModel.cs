using System.ComponentModel.DataAnnotations;

namespace MyBlogs.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "The Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The Email is required")]
        [EmailAddress(ErrorMessage = "The Email is not valid")]
        public string Email {  get; set; }

        [Required(ErrorMessage = "The password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
