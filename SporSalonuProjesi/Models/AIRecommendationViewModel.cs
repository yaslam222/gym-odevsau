using System.ComponentModel.DataAnnotations;

namespace SporSalonuProjesi.Models
{
    public class AIRecommendationViewModel
    {
        [Required(ErrorMessage = "Boy girilmesi zorunludur")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public double Height { get; set; }

        [Required(ErrorMessage = "Kilo girilmesi zorunludur")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        public double Weight { get; set; }

        [Required(ErrorMessage = "Vücut tipi seçilmesi zorunludur")]
        [Display(Name = "Vücut Tipi")]
        public string BodyType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fitness hedefi seçilmesi zorunludur")]
        [Display(Name = "Fitness Hedefi")]
        public string FitnessGoal { get; set; } = string.Empty;

        [Display(Name = "Vücut Fotoğrafı (Opsiyonel)")]
        public IFormFile? BodyImage { get; set; }

        [Display(Name = "Plan Tipi")]
        public string RecommendationType { get; set; } = "workout"; // workout, diet, both

        // Results
        public string? WorkoutRecommendation { get; set; }
        public string? DietRecommendation { get; set; }
        public string? ImageAnalysis { get; set; }
        public string? FutureBodyImageUrl { get; set; }
    }
}
