using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Login is required")]
        [Display(Name = "Login")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Retyping your password is required")]
        [Display(Name = "Retype Password")]
        [DataType(DataType.Password)]
        public string RetypedPassword { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }
    }
}