using System.ComponentModel.DataAnnotations;

namespace SESM.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Login is required")]
        [Display(Name = "Login")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
  
    }
}