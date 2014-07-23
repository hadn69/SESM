using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SESM.Models.Views.Mod
{
    public class UploadModViewModel
    {
        [Required(ErrorMessage = "Mod Zip is required")]
        [Display(Name = "Mod Zip")]
        public HttpPostedFileBase ModZip { get; set; }
    }
}