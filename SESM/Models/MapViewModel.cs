using System.ComponentModel.DataAnnotations;

namespace SESM.Models
{
    public class MapViewModel
    {
        [Required(ErrorMessage = "Map Name is required")]
        [Display(Name = "Map Name")]
        public string MapName { get; set; }
    }

    public class NewMapViewModel
    {

        [Required(ErrorMessage = "Scenario Type is required")]
        [Display(Name = "Scenario")]
        public SubTypeId MapType { get; set; }

        [Required(ErrorMessage = "Asteroid Amount is required")]
        [Display(Name = "Asteroid Amount")]
        public int AsteroidAmount { get; set; }

        public NewMapViewModel()
        {
            MapType = SubTypeId.EasyStart1;
            AsteroidAmount = 4;
        }
    }
}