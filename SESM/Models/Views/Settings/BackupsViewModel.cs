using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Settings
{
    public class BackupsViewModel
    {
        [Required]
        [Display(Name = "Activate Backup 1")]
        public bool EnableLvl1 { get; set; }

        [Required]
        [Display(Name = "Backup to keep 1")]
        public int NbToKeepLvl1 { get; set; }

        [Required]
        [Display(Name = "Cron Interval 1")]
        public string CronIntervalLvl1 { get; set; }

        [Required]
        [Display(Name = "Activate Backup 2")]
        public bool EnableLvl2 { get; set; }

        [Required]
        [Display(Name = "Backup to keep 2")]
        public int NbToKeepLvl2 { get; set; }

        [Required]
        [Display(Name = "Cron Interval 2")]
        public string CronIntervalLvl2 { get; set; }

        [Required]
        [Display(Name = "Activate Backup 3")]
        public bool EnableLvl3 { get; set; }

        [Required]
        [Display(Name = "Backup to keep 3")]
        public int NbToKeepLvl3 { get; set; }

        [Required]
        [Display(Name = "Cron Interval 3")]
        public string CronIntervalLvl3 { get; set; }
    }
}