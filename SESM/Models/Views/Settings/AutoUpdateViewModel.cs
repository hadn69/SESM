using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Settings
{
    public class AutoUpdateViewModel
    {
        [Required]
        [Display(Name = "Activate Auto Update")]
        public bool AutoUpdate { get; set; }

        [Required]
        [Display(Name = "Steam Username")]
        public string UserName { get; set; }

        [Display(Name = "Steam Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Cron Interval")]
        public string CronInterval { get; set; }
    }
}