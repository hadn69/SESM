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
        [Display(Name = "Initial Asteroid Amount")]
        [Range(0, 300, ErrorMessage = "Asteroid Amount must be between 0 and 300")]
        public int AsteroidAmount { get; set; }

        [Required(ErrorMessage = "Procedural Density is required")]
        [Display(Name = "Procedural Density")]
        [Range(0, 1, ErrorMessage = "Procedural Density must be between 0 and 1")]
        public float ProceduralDensity { get; set; }

        [Required(ErrorMessage = "Procedural Seed is required")]
        [Display(Name = "Procedural Seed")]
        public int ProceduralSeed { get; set; }

        public NewMapViewModel()
        {
            MapType = SubTypeId.EasyStart1;
            AsteroidAmount = 4;
            ProceduralDensity = 0;
            ProceduralSeed = 0;
        }
    }
}