using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Map
{
    public class RenameMapViewModel
    {

        [Display(Name = "Current Map Name")]
        public string CurrentMapName { get; set; }

        [Required(ErrorMessage = "New Map Name")]
        [Display(Name = "New Map Name")]
        [RegularExpression(@"^[a-zA-Z0-9_. -]+$", ErrorMessage = "The New Map Name must be only composed of letters, numbers, dots, dashs, spaces and underscores")]
        public string NewMapName { get; set; }

    }
}