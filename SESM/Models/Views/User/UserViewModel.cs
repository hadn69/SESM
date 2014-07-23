using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.User
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "Login is required")]
        [Display(Name = "Login")]
        public string Login { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Super Admin is required")]
        [Display(Name = "Super Admin")]
        public bool IsAdmin { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }
    }
}