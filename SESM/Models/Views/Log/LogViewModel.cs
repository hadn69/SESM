using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Log
{
    public class LogViewModel
    {
        [Required(ErrorMessage = "Log Name is required")]
        [Display(Name = "Log Name")]
        public string LogName { get; set; }
    }
}