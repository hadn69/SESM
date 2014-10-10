using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Mod
{
    public class ModViewModel
    {
        [Required(ErrorMessage = "Mod list is required")]
        [Display(Name = "Mod List")]
        public List<string> ModName { get; set; }

        public ModViewModel()
        {
            ModName = new List<string>();
        }
    }
}