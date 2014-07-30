using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Settings
{
    public class SettingsViewModel
    {
        [Required]
        [Display(Name = "Prefix")]
        public string Prefix { get; set; }

        [Required]
        [Display(Name = "Save Path")]
        public string SESavePath { get; set; }

        [Required]
        [Display(Name = "Server Path")]
        public string SEDataPath { get; set; }

        [Required]
        [Display(Name = "System")]
        public EnumArchType EnumArch { get; set; }

        [Required]
        [Display(Name = "Add Date To Logs")]
        public bool AddDateToLog { get; set; }

        [Required]
        [Display(Name = "Send Log To KSH")]
        public bool SendLogToKeen { get; set; }
    }
}