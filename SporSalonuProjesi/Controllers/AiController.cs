using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // <-- Hata kaydı (Logging) için EKLENDİ
using SporSalonuProjesi.Service; // IAIRecommendationService arayüzünüz
using System; // <-- Exception (try-catch) için EKLENDİ
using System.Threading.Tasks;

namespace SporSalonuProjesi.Controllers
{
    [Authorize] // Sadece üyeler erişebilsin
    public class AiController : Controller
    {
        private readonly IAIRecommendationService _aiService;
        private readonly ILogger<AiController> _logger; // <-- LOGGER EKLENDİ

        // DÜZELTME: Constructor'a ILogger eklendi
        public AiController(IAIRecommendationService aiService, ILogger<AiController> logger)
        {
            _aiService = aiService;
            _logger = logger; // <-- LOGGER EKLENDİ
        }

        // GET: /Ai/Index (GET /Recommendation/Index değil)
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetPlan(double height, double weight, string bodyType, string fitnessGoal)
        {
            if (height <= 0 || weight <= 0 || string.IsNullOrEmpty(bodyType) || string.IsNullOrEmpty(fitnessGoal))
            {
                return Json(new { success = false, message = "Lütfen tüm alanları doldurun." });
            }

            // --- DÜZELTME: Hata yakalamak için try-catch eklendi ---
            try
            {
                // Bu servisler yavaş olabilir ve hata verebilir
                string workoutPlan = await _aiService.GetWorkoutRecommendationAsync(height, weight, bodyType, fitnessGoal);

                string dietPlan = await _aiService.GetDietPlanAsync(height, weight, bodyType, fitnessGoal);

                string fullPlan = $"### 🏋️ Antrenman Planı\n{workoutPlan}\n\n<hr>\n\n### 🍎 Diyet Planı\n{dietPlan}";

                return Json(new { success = true, plan = fullPlan });
            }
            catch (Exception ex)
            {
                // Bir hata olursa, hatayı sunucuya kaydet (logla)
                _logger.LogError(ex, "AI servisinde (GetPlan) bir hata oluştu. Parametreler: {height}, {weight}, {bodyType}, {fitnessGoal}", height, weight, bodyType, fitnessGoal);

                // Kullanıcıya 500 hatası döndürmek yerine,
                // kontrollü bir 'başarısız' JSON mesajı döndür.
                return Json(new { success = false, message = "Yapay zeka servisiyle iletişim kurulamadı. Lütfen daha sonra tekrar deneyin." });
            }
        }
    }
}