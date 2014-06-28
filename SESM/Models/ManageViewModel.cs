using System.ComponentModel.DataAnnotations;

namespace SESM.Models
{
    public class ManageViewModel
    {
        [Display(Name = "Old Password")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Retype New Password")]
        [DataType(DataType.Password)]
        public string RetypedPassword { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}