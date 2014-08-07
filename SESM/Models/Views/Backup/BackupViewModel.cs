using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Backup
{
    public class BackupViewModel
    {
        [Required(ErrorMessage = "Backup Name is required")]
        [Display(Name = "Backup Name")]
        public string BackupName { get; set; }
    }
}