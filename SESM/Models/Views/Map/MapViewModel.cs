using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Map
{
    public class MapViewModel
    {
        [Required(ErrorMessage = "Map Name is required")]
        [Display(Name = "Map Name")]
        public string MapName { get; set; }
    }
}