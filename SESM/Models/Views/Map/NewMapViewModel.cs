using System.ComponentModel.DataAnnotations;
using SESM.Models.Views.Server;

namespace SESM.Models.Views.Map
{
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