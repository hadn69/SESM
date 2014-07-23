using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Mod
{
    public class ModViewModel
    {
        [Required(ErrorMessage = "Mod Name is required")]
        [Display(Name = "Mod Name")]
        public string ModName { get; set; }
    }
}