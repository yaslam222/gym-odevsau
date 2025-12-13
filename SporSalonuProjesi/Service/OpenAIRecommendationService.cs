using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SporSalonuProjesi.Service;

namespace SporSalonuProjesi.Services
{
    public class OpenAIRecommendationService : IAIRecommendationService
    {
        private readonly string _apiKey;
        private readonly RestClient _client;
        private readonly ILogger<OpenAIRecommendationService> _logger;

        public OpenAIRecommendationService(IConfiguration configuration, ILogger<OpenAIRecommendationService> logger)
        {
            _apiKey = configuration["OpenAI:ApiKey"];
            _client = new RestClient("https://api.openai.com/v1");
            _logger = logger;

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("OpenAI API Key bulunamadı. Lütfen appsettings.json dosyasını kontrol edin.");
            }

            _logger.LogInformation("SimpleOpenAIService başlatıldı");
        }

        private async Task<string> CallChatGPTAsync(string systemMessage, string userMessage)
        {
            try
            {
                _logger.LogInformation("ChatGPT API çağrısı yapılıyor...");

                var request = new RestRequest("chat/completions", RestSharp.Method.Post);
                request.AddHeader("Authorization", $"Bearer {_apiKey}");
                request.AddHeader("Content-Type", "application/json");

                var body = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = systemMessage },
                        new { role = "user", content = userMessage }
                    },
                    temperature = 0.7,
                    max_tokens = 2000
                };

                request.AddJsonBody(body);

                _logger.LogInformation("API isteği gönderiliyor...");
                var response = await _client.ExecuteAsync(request);

                _logger.LogInformation("API yanıtı alındı. Status: {Status}", response.StatusCode);

                if (!response.IsSuccessful)
                {
                    var errorContent = response.Content ?? "Bilinmeyen hata";
                    _logger.LogError("OpenAI API hatası: {Error}", errorContent);
                    throw new Exception($"OpenAI API Error: {response.StatusCode} - {errorContent}");
                }

                if (string.IsNullOrEmpty(response.Content))
                {
                    throw new Exception("OpenAI API boş yanıt döndü");
                }

                var jsonResponse = JObject.Parse(response.Content);
                var content = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString();

                if (string.IsNullOrEmpty(content))
                {
                    throw new Exception("OpenAI API yanıtında içerik bulunamadı");
                }

                _logger.LogInformation("ChatGPT başarıyla yanıt verdi");
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChatGPT çağrısında hata");
                throw new Exception($"AI hizmeti hatası: {ex.Message}", ex);
            }
        }

        public async Task<string> GetWorkoutRecommendationAsync(double height, double weight, string bodyType, string fitnessGoal)
        {
            try
            {
                var bmi = weight / Math.Pow(height / 100, 2);

                var systemMessage = "Sen profesyonel bir fitness koçusun. Detaylı ve kişiselleştirilmiş egzersiz programları hazırlıyorsun.";

                var userMessage = $@"Aşağıdaki bilgilere sahip bir kullanıcı için detaylı egzersiz programı hazırla:

Boy: {height} cm
Kilo: {weight} kg
BMI: {bmi:F2}
Vücut Tipi: {bodyType}
Hedef: {fitnessGoal}

Lütfen şunları içeren bir program hazırla:
1. Haftalık egzersiz planı (hangi günler hangi kas grupları)
2. Her egzersiz için set ve tekrar sayıları
3. Önerilen kardiyo aktiviteleri
4. Dinlenme günleri
5. İlerleme için öneriler

Türkçe ve detaylı bir şekilde açıkla.";

                return await CallChatGPTAsync(systemMessage, userMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Egzersiz önerisi oluşturma hatası");
                throw;
            }
        }

        public async Task<string> GetDietPlanAsync(double height, double weight, string bodyType, string fitnessGoal)
        {
            try
            {
                var bmi = weight / Math.Pow(height / 100, 2);

                var systemMessage = "Sen profesyonel bir diyetisyensin. Sağlıklı ve uygulanabilir beslenme planları oluşturuyorsun.";

                var userMessage = $@"Aşağıdaki bilgilere sahip bir kullanıcı için detaylı diyet planı hazırla:

Boy: {height} cm
Kilo: {weight} kg
BMI: {bmi:F2}
Vücut Tipi: {bodyType}
Hedef: {fitnessGoal}

Lütfen şunları içeren bir plan hazırla:
1. Günlük kalori ihtiyacı
2. Makro besin dağılımı (protein, karbonhidrat, yağ)
3. Örnek günlük öğün planı (kahvaltı, öğle, akşam, ara öğünler)
4. Su tüketimi önerisi
5. Kaçınılması gereken yiyecekler
6. Önerilen takviyeler (varsa)

Türkçe ve detaylı bir şekilde açıkla.";

                return await CallChatGPTAsync(systemMessage, userMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Diyet planı oluşturma hatası");
                throw;
            }
        }

        public async Task<string> AnalyzeBodyImageAsync(IFormFile image, string fitnessGoal)
        {
            try
            {
                _logger.LogInformation("Vücut fotoğrafı analizi başlıyor...");

                // GPT-4 Vision için
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);

                var request = new RestRequest("chat/completions", RestSharp.Method.Post);
                request.AddHeader("Authorization", $"Bearer {_apiKey}");
                request.AddHeader("Content-Type", "application/json");

                var body = new
                {
                    model = "gpt-4-vision-preview",
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new { type = "text", text = $@"Bu vücut fotoğrafını analiz et ve şu bilgileri ver:
1. Genel vücut tipi değerlendirmesi
2. Güçlü ve zayıf kas grupları
3. '{fitnessGoal}' hedefine ulaşmak için odaklanılması gereken alanlar
4. Önerilen egzersizler
5. Tahmini başlangıç seviyesi

Türkçe, detaylı ve motive edici bir dille açıkla." },
                                new
                                {
                                    type = "image_url",
                                    image_url = new { url = $"data:image/jpeg;base64,{base64Image}" }
                                }
                            }
                        }
                    },
                    max_tokens = 1000
                };

                request.AddJsonBody(body);

                var response = await _client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    throw new Exception($"OpenAI Vision API Error: {response.StatusCode} - {response.Content}");
                }

                var jsonResponse = JObject.Parse(response.Content);
                return jsonResponse["choices"][0]["message"]["content"].ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fotoğraf analizi hatası");
                throw new Exception($"Fotoğraf analizi başarısız: {ex.Message}", ex);
            }
        }

        public async Task<string> GenerateFutureBodyImageAsync(string currentDescription, string targetGoal, int timeframe)
        {
            try
            {
                _logger.LogInformation("DALL-E ile görsel oluşturuluyor...");

                var prompt = $@"Realistic fitness transformation photography: A person with {currentDescription} body type 
after {timeframe} months of consistent training and proper nutrition, successfully achieving their goal of {targetGoal}. 
Professional gym photography style, natural lighting, confident athletic pose, high quality, motivational and inspiring.";

                var request = new RestRequest("images/generations", RestSharp.Method.Post);
                request.AddHeader("Authorization", $"Bearer {_apiKey}");
                request.AddHeader("Content-Type", "application/json");

                var body = new
                {
                    model = "dall-e-3",
                    prompt = prompt,
                    n = 1,
                    size = "1024x1024",
                    quality = "hd",
                    style = "natural"
                };

                request.AddJsonBody(body);

                var response = await _client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    throw new Exception($"DALL-E API Error: {response.StatusCode} - {response.Content}");
                }

                var jsonResponse = JObject.Parse(response.Content);
                return jsonResponse["data"][0]["url"].ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DALL-E görsel oluşturma hatası");
                throw new Exception($"Görsel oluşturulamadı: {ex.Message}", ex);
            }
        }
    }
}