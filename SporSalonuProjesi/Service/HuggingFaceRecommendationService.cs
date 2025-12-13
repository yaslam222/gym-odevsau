using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SporSalonuProjesi.Service; // IAIRecommendationService arayüzü burada
using System.Text;
using System.Text.Json; // JsonSerializer için gerekli

namespace SporSalonuProjesi.Services // Servislerinizin namespace'i
{
    // Bu yardımcı sınıfı, ana sınıfın DIŞINA (ama namespace'in içine) taşıdım
    public class HuggingFaceResponse
    {
        public string generated_text { get; set; }
    }

    public class HuggingFaceRecommendationService : IAIRecommendationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HuggingFaceRecommendationService> _logger;

        // HuggingFace'teki ücretsiz, metin-metne model
       
        private const string TextApiUrl = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.2";
        public HuggingFaceRecommendationService(IHttpClientFactory httpClientFactory, ILogger<HuggingFaceRecommendationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // --- DÜZELTME 1: GetWorkoutRecommendationAsync (Metod adı ve parametreler düzeltildi) ---
        public async Task<string> GetWorkoutRecommendationAsync(double height, double weight, string bodyType, string fitnessGoal)
        {
            _logger.LogInformation("HuggingFace servisinden ANTRNEMAN önerisi isteniyor...");

            try
            {
                // Prompt'u (istek metni) arayüzdeki yeni parametrelere göre güncelledim
                string prompt = $"Create a simple 3-day exercise plan for a person with body type '{bodyType}', {weight}kg, {height}cm tall, whose goal is '{fitnessGoal}'.";

                // API isteği
                var payload = new { inputs = prompt };
                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var httpClient = _httpClientFactory.CreateClient("HuggingFaceClient");
                var response = await httpClient.PostAsync(TextApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"HuggingFace API hatası: {response.StatusCode}");
                    return "Yapay zeka servisi şu anda yanıt veremiyor (Hata Kodu: " + response.StatusCode + ").";
                }

                // Yanıtı işle
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<HuggingFaceResponse>>(jsonResponse);

                if (results != null && results.Count > 0 && !string.IsNullOrEmpty(results[0].generated_text))
                {
                    _logger.LogInformation("HuggingFace servisinden yanıt alındı.");
                    return results[0].generated_text;
                }

                return "Yapay zeka bir yanıt üretemedi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HuggingFace servisinde kritik hata.");
                return "Öneri alınırken bir hata oluştu: " + ex.Message;
            }
        }

        // --- DÜZELTME 2: GetDietPlanAsync (Eksik metod eklendi) ---
        // Bu metod da aynı metin modelini, farklı bir prompt ile kullanabilir.
        public async Task<string> GetDietPlanAsync(double height, double weight, string bodyType, string fitnessGoal)
        {
            _logger.LogInformation("HuggingFace servisinden DİYET önerisi isteniyor...");

            try
            {
                // Prompt'u diyet planına göre değiştir
                string prompt = $"Create a simple 3-day diet plan for a person with body type '{bodyType}', {weight}kg, {height}cm tall, whose goal is '{fitnessGoal}'.";

                var payload = new { inputs = prompt };
                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var httpClient = _httpClientFactory.CreateClient("HuggingFaceClient");
                var response = await httpClient.PostAsync(TextApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    return "Yapay zeka servisi şu anda yanıt veremiyor (Hata Kodu: " + response.StatusCode + ").";
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<HuggingFaceResponse>>(jsonResponse);

                if (results != null && results.Count > 0 && !string.IsNullOrEmpty(results[0].generated_text))
                {
                    return results[0].generated_text;
                }

                return "Yapay zeka bir yanıt üretemedi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HuggingFace servisinde kritik hata.");
                return "Öneri alınırken bir hata oluştu: " + ex.Message;
            }
        }


        // --- DÜZELTME 3 & 4: Resim Metodları (%100 ÜCRETSİZ kuralı) ---
        // Ücretsiz API'ler resim analizi veya oluşturmayı desteklemez.
        // Bu yüzden bu metodları, arayüzü tatmin etmek için "uygulanmadı" olarak işaretliyoruz.

        public Task<string> AnalyzeBodyImageAsync(IFormFile image, string fitnessGoal)
        {
            _logger.LogWarning("AnalyzeBodyImageAsync çağrıldı ancak ücretsiz modelde desteklenmiyor.");
            string message = "Resim analizi özelliği, %100 ücretsiz API'de mevcut değildir. Bu özellik için ücretli bir servis (OpenAI, vb.) gereklidir.";
            return Task.FromResult(message);
        }

        public Task<string> GenerateFutureBodyImageAsync(string currentDescription, string targetGoal, int timeframe)
        {
            _logger.LogWarning("GenerateFutureBodyImageAsync çağrıldı ancak ücretsiz modelde desteklenmiyor.");
            string message = "Resim oluşturma özelliği, %100 ücretsiz API'de mevcut değildir. Bu özellik için ücretli bir servis (OpenAI, DALL-E, vb.) gereklidir.";
            return Task.FromResult(message);
        }
    }
}