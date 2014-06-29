using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SESM.Models
{
    public class UploadBinViewModel
    {
        [Required(ErrorMessage = "Dedicated Server Zip is required")]
        [Display(Name = "Dedicated Server Zip")]
        public HttpPostedFileBase ServerZip { get; set; }
    }
}