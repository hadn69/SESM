using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Settings
{
    public class SteamCMDViewModel
    {
        [Required]
        [Display(Name = "Steam Username")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Steam Password")]
        public string Password { get; set; }

        [Display(Name = "Steam Guard")]
        public string SteamGuard { get; set; }

    }
}