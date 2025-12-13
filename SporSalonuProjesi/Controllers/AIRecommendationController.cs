using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SporSalonuProjesi.Models;
using SporSalonuProjesi.Service;

namespace SporSalonuProjesi.Controllers
{
    [Authorize]
    public class AIRecommendationController : Controller
    {
        private readonly IAIRecommendationService _aiService;
        private readonly ILogger<AIRecommendationController> _logger;

        public AIRecommendationController(
            IAIRecommendationService aiService,
            ILogger<AIRecommendationController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new AIRecommendationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetRecommendation(AIRecommendationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // Egzersiz önerisi al
                if (model.RecommendationType == "workout" || model.RecommendationType == "both")
                {
                    model.WorkoutRecommendation = await _aiService.GetWorkoutRecommendationAsync(
                        model.Height,
                        model.Weight,
                        model.BodyType,
                        model.FitnessGoal
                    );
                }

                // Diyet planı al
                if (model.RecommendationType == "diet" || model.RecommendationType == "both")
                {
                    model.DietRecommendation = await _aiService.GetDietPlanAsync(
                        model.Height,
                        model.Weight,
                        model.BodyType,
                        model.FitnessGoal
                    );
                }

                // Fotoğraf analizi (eğer yüklendiyse)
                if (model.BodyImage != null && model.BodyImage.Length > 0)
                {
                    model.ImageAnalysis = await _aiService.AnalyzeBodyImageAsync(
                        model.BodyImage,
                        model.FitnessGoal
                    );
                }

                TempData["SuccessMessage"] = "AI önerileri başarıyla oluşturuldu!";
                return View("Results", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI öneri oluşturma hatası");
                ModelState.AddModelError("", "Öneriler oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateFutureImage(string currentDescription, string targetGoal, int timeframe)
        {
            try
            {
                var imageUrl = await _aiService.GenerateFutureBodyImageAsync(
                    currentDescription,
                    targetGoal,
                    timeframe
                );

                return Json(new { success = true, imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gelecek vücut görseli oluşturma hatası");
                return Json(new { success = false, message = "Görsel oluşturulamadı." });
            }
        }
    }
}