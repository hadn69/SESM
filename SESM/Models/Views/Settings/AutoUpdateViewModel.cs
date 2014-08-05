using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Settings
{
    public class AutoUpdateViewModel
    {
        [Required]
        [Display(Name = "Steam Username")]
        public string UserName { get; set; }

        [Display(Name = "Steam Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Auto update every 10mn")]
        public bool AutoUpdate { get; set; }

    }
}