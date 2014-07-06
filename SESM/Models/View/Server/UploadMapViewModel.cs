using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SESM.Models.View.Server
{
    public class UploadMapViewModel
    {
        [Required(ErrorMessage = "Save name is required")]
        [Display(Name = "Save Name")]
        [RegularExpression(@"^[a-zA-Z0-9_. -]+$", ErrorMessage = "The save name must be only composed of letters, numbers, dots, dashes, spaces and underscores")]
        public string SaveName { get; set; }

        [Required(ErrorMessage = "Save Zip is required")]
        [Display(Name = "Save Zip")]
        public HttpPostedFileBase SaveZip { get; set; }
    }
}