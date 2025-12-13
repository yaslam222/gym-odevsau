using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SporSalonuProjesi.Service;
using System;
using System.Threading.Tasks;

namespace SporSalonuProjesi.Services
{
    // BASIT ŞABLON SİSTEMİ - Yapay zeka olmadan çalışır
    public class SimpleRecommendationService : IAIRecommendationService
    {
        private readonly ILogger<SimpleRecommendationService> _logger;

        public SimpleRecommendationService(ILogger<SimpleRecommendationService> logger)
        {
            _logger = logger;
        }

        public Task<string> GetWorkoutRecommendationAsync(double height, double weight, string bodyType, string fitnessGoal)
        {
            _logger.LogInformation($"DİNAMİK plan hesaplanıyor: {height}cm, {weight}kg, {bodyType}, {fitnessGoal}");

            // BMI Hesapla
            double heightInMeters = height / 100.0;
            double bmi = weight / (heightInMeters * heightInMeters);

            // Fitness seviyesini belirle
            string fitnessLevel = GetFitnessLevel(bmi, bodyType);

            // Hedefine göre dinamik set/tekrar hesapla
            var (sets, reps, restTime, cardioMinutes) = CalculateWorkoutParameters(bmi, fitnessGoal, bodyType);

            string plan = $@"**KIŞISELLEŞTIRILMIŞ ANTRENMAN PLANI**
📊 Sizin İçin Hesaplanan Değerler:
• Boy: {height} cm
• Kilo: {weight} kg
• BMI: {bmi:F1} ({GetBMICategory(bmi)})
• Vücut Tipi: {bodyType}
• Hedef: {fitnessGoal}
• Fitness Seviyesi: {fitnessLevel}

---

**GÜN 1 - {GetDay1Focus(fitnessGoal)}**
{GenerateDay1Exercises(sets, reps, restTime, fitnessGoal, bmi)}

**GÜN 2 - {GetDay2Focus(fitnessGoal)}**
{GenerateDay2Exercises(sets, reps, restTime, cardioMinutes, bmi)}

**GÜN 3 - {GetDay3Focus(fitnessGoal)}**
{GenerateDay3Exercises(sets, reps, restTime, fitnessGoal, bmi)}

---

💡 **Özel Notlar:**
{GeneratePersonalizedTips(bmi, fitnessGoal, bodyType, weight)}";

            return Task.FromResult(plan);
        }

        public Task<string> GetDietPlanAsync(double height, double weight, string bodyType, string fitnessGoal)
        {
            _logger.LogInformation($"DİNAMİK diyet hesaplanıyor: {height}cm, {weight}kg, {bodyType}, {fitnessGoal}");

            // BMR (Bazal Metabolizma Hızı) Hesapla - Mifflin-St Jeor denklemi (erkek için)
            double heightInCm = height;
            double bmr = (10 * weight) + (6.25 * heightInCm) - (5 * 30) + 5; // 30 yaş varsayımı

            // Günlük kalori ihtiyacı (aktivite faktörü ile)
            double activityFactor = GetActivityFactor(fitnessGoal);
            double dailyCalories = bmr * activityFactor;

            // Hedefe göre kaloriyi ayarla
            dailyCalories = AdjustCaloriesForGoal(dailyCalories, fitnessGoal);

            // Makro besinleri hesapla
            var (protein, carbs, fats) = CalculateMacros(weight, fitnessGoal);

            // Öğün başına kalori
            int breakfast = (int)(dailyCalories * 0.30);
            int lunch = (int)(dailyCalories * 0.35);
            int dinner = (int)(dailyCalories * 0.25);
            int snacks = (int)(dailyCalories * 0.10);

            string plan = $@"**KIŞISELLEŞTIRILMIŞ DİYET PLANI**
📊 Sizin İçin Hesaplanan Değerler:
• Boy: {height} cm
• Kilo: {weight} kg
• BMR (Bazal Metabolik): {bmr:F0} kalori
• Günlük Hedef: {dailyCalories:F0} kalori
• Protein: {protein}g | Karbonhidrat: {carbs}g | Yağ: {fats}g

---

**GÜN 1**
🌅 **Kahvaltı (~{breakfast} kal)**
{GenerateBreakfast(breakfast, protein / 3, fitnessGoal)}

🍽️ **Öğle Yemeği (~{lunch} kal)**
{GenerateLunch(lunch, protein / 3, carbs / 3, fitnessGoal)}

🌙 **Akşam Yemeği (~{dinner} kal)**
{GenerateDinner(dinner, protein / 3, fitnessGoal)}

🥤 **Ara Öğünler (~{snacks} kal)**
{GenerateSnacks(snacks, fitnessGoal)}

---

**GÜN 2**
🌅 **Kahvaltı (~{breakfast} kal)**
{GenerateBreakfast2(breakfast, protein / 3)}

🍽️ **Öğle Yemeği (~{lunch} kal)**
{GenerateLunch2(lunch, protein / 3, carbs / 3)}

🌙 **Akşam Yemeği (~{dinner} kal)**
{GenerateDinner2(dinner, protein / 3)}

🥤 **Ara Öğünler (~{snacks} kal)**
{GenerateSnacks2(snacks)}

---

**GÜN 3**
🌅 **Kahvaltı (~{breakfast} kal)**
{GenerateBreakfast3(breakfast, protein / 3)}

🍽️ **Öğle Yemeği (~{lunch} kal)**
{GenerateLunch3(lunch, protein / 3, carbs / 3, fitnessGoal)}

🌙 **Akşam Yemeği (~{dinner} kal)**
{GenerateDinner3(dinner, protein / 3)}

🥤 **Ara Öğünler (~{snacks} kal)**
{GenerateSnacks3(snacks)}

---

💡 **Beslenme Önerileri:**
{GenerateDietTips(dailyCalories, weight, fitnessGoal)}

💧 **Günlük Su İhtiyacı:** {CalculateWaterIntake(weight)} litre";

            return Task.FromResult(plan);
        }

        // ===== YARDIMCI HESAPLAMA METODLARı =====

        private string GetFitnessLevel(double bmi, string bodyType)
        {
            if (bmi < 18.5) return "Başlangıç (Düşük BMI)";
            if (bmi < 25) return bodyType == "Atletik" ? "İleri" : "Orta";
            if (bmi < 30) return "Başlangıç-Orta";
            return "Başlangıç";
        }

        private string GetBMICategory(double bmi)
        {
            if (bmi < 18.5) return "Zayıf";
            if (bmi < 25) return "Normal";
            if (bmi < 30) return "Fazla Kilolu";
            return "Obez";
        }

        private (int sets, int reps, int restTime, int cardioMinutes) CalculateWorkoutParameters(double bmi, string fitnessGoal, string bodyType)
        {
            int sets = 3, reps = 12, restTime = 60, cardioMinutes = 20;

            if (fitnessGoal.ToLower().Contains("kas") || fitnessGoal.ToLower().Contains("muscle"))
            {
                sets = 4;
                reps = bmi < 25 ? 8 : 10;
                restTime = 90;
                cardioMinutes = 10;
            }
            else if (fitnessGoal.ToLower().Contains("kilo") || fitnessGoal.ToLower().Contains("weight"))
            {
                sets = 3;
                reps = 15;
                restTime = 45;
                cardioMinutes = bmi > 30 ? 30 : 25;
            }
            else // Fit olmak
            {
                sets = 3;
                reps = 12;
                restTime = 60;
                cardioMinutes = 20;
            }

            return (sets, reps, restTime, cardioMinutes);
        }

        private double GetActivityFactor(string fitnessGoal)
        {
            if (fitnessGoal.ToLower().Contains("kas")) return 1.725; // Çok aktif
            if (fitnessGoal.ToLower().Contains("kilo")) return 1.55; // Aktif
            return 1.55; // Orta aktif
        }

        private double AdjustCaloriesForGoal(double calories, string fitnessGoal)
        {
            if (fitnessGoal.ToLower().Contains("kas")) return calories + 300; // Fazlalık oluştur
            if (fitnessGoal.ToLower().Contains("kilo")) return calories - 500; // Açık oluştur
            return calories; // Koruma
        }

        private (int protein, int carbs, int fats) CalculateMacros(double weight, string fitnessGoal)
        {
            int protein, carbs, fats;

            if (fitnessGoal.ToLower().Contains("kas"))
            {
                protein = (int)(weight * 2.2); // 2.2g/kg
                fats = (int)(weight * 1.0);
                carbs = (int)(weight * 4.0);
            }
            else if (fitnessGoal.ToLower().Contains("kilo"))
            {
                protein = (int)(weight * 2.0);
                fats = (int)(weight * 0.8);
                carbs = (int)(weight * 2.0);
            }
            else // Fit
            {
                protein = (int)(weight * 1.8);
                fats = (int)(weight * 0.9);
                carbs = (int)(weight * 3.0);
            }

            return (protein, carbs, fats);
        }

        private double CalculateWaterIntake(double weight)
        {
            return Math.Round(weight * 0.033, 1); // 33ml per kg
        }

        // ===== EGZERSİZ OLUŞTURUCU METODLAR =====

        private string GetDay1Focus(string goal)
        {
            if (goal.ToLower().Contains("kas")) return "Göğüs & Triceps (Kas Kütlesi)";
            if (goal.ToLower().Contains("kilo")) return "Full Body Circuit (Yağ Yakımı)";
            return "Upper Body (Üst Vücut)";
        }

        private string GetDay2Focus(string goal)
        {
            if (goal.ToLower().Contains("kas")) return "Sırt & Biceps (Güç)";
            if (goal.ToLower().Contains("kilo")) return "Kardiyo Yoğun + Core";
            return "Lower Body (Alt Vücut)";
        }

        private string GetDay3Focus(string goal)
        {
            if (goal.ToLower().Contains("kas")) return "Bacak & Omuz (Kuvvet)";
            if (goal.ToLower().Contains("kilo")) return "HIIT + Full Body";
            return "Full Body + Kardiyo";
        }

        private string GenerateDay1Exercises(int sets, int reps, int rest, string goal, double bmi)
        {
            if (goal.ToLower().Contains("kas"))
            {
                return $@"• Bench Press: {sets} set x {reps} tekrar (Dinlenme: {rest}sn)
• Incline Dumbbell Press: {sets} set x {reps + 2} tekrar
• Cable Fly: {sets - 1} set x {reps + 3} tekrar
• Triceps Dips: {sets} set x {reps} tekrar
• Triceps Pushdown: {sets} set x {reps + 2} tekrar
⏱️ Toplam Süre: ~45 dakika";
            }
            else if (goal.ToLower().Contains("kilo"))
            {
                int circuitRounds = bmi > 30 ? 3 : 4;
                return $@"**Circuit ({circuitRounds} tur):**
• Jump Squats: {reps + 5} tekrar
• Push-ups: {reps} tekrar
• Mountain Climbers: {reps + 8} tekrar
• Burpees: {reps - 2} tekrar
• Plank: {rest - 10} saniye
⏱️ Tur arası dinlenme: 90 saniye
⏱️ Toplam Süre: ~35 dakika";
            }
            else
            {
                return $@"• Push-ups: {sets} set x {reps} tekrar
• Dumbbell Bench Press: {sets} set x {reps} tekrar
• Shoulder Press: {sets} set x {reps - 2} tekrar
• Lateral Raises: {sets} set x {reps + 3} tekrar
• Triceps Extension: {sets} set x {reps} tekrar
⏱️ Toplam Süre: ~40 dakika";
            }
        }

        private string GenerateDay2Exercises(int sets, int reps, int rest, int cardio, double bmi)
        {
            return $@"• Isınma: 5 dakika hafif kardiyo
• Squat: {sets} set x {reps} tekrar
• Lunges: {sets} set x {reps} tekrar (her bacak)
• Leg Press: {sets} set x {reps + 2} tekrar
• Leg Curl: {sets} set x {reps} tekrar
• Calf Raises: {sets} set x {reps + 5} tekrar
• Kardiyo: {cardio} dakika (koşu bandı/bisiklet)
⏱️ Toplam Süre: ~{40 + cardio} dakika";
        }

        private string GenerateDay3Exercises(int sets, int reps, int rest, string goal, double bmi)
        {
            if (goal.ToLower().Contains("kilo"))
            {
                return $@"**HIIT Protokolü (20 saniye çalış / 10 saniye dinlen x 8 tur):**
• Sprint / Hızlı Koşu
• Box Jumps
• Kettlebell Swings
• Battle Ropes
**Full Body Finish:**
• Plank to Push-up: 3 set x 10 tekrar
• Bicycle Crunches: 3 set x 20 tekrar
⏱️ Toplam Süre: ~30 dakika (yoğun!)";
            }
            else
            {
                return $@"• Deadlift: {sets} set x {reps - 4} tekrar (AĞIR)
• Pull-ups: {sets} set x max tekrar
• Barbell Row: {sets} set x {reps} tekrar
• Dumbbell Curl: {sets} set x {reps} tekrar
• Hammer Curl: {sets} set x {reps} tekrar
• Core: Plank {rest}sn + Russian Twist 20 tekrar
⏱️ Toplam Süre: ~45 dakika";
            }
        }

        private string GeneratePersonalizedTips(double bmi, string goal, string bodyType, double weight)
        {
            string tips = "";

            if (bmi < 18.5)
                tips += "• Kilo almanız gerekiyor, bu yüzden ağırlık antrenmanlarına odaklanın.\n";
            else if (bmi > 30)
                tips += "• Önce kardiyo ile yağ yakımına odaklanın, sonra ağırlık ekleyin.\n";

            if (goal.ToLower().Contains("kas"))
                tips += "• Progressive overload: Her hafta ağırlığı %2-5 artırın.\n";
            else if (goal.ToLower().Contains("kilo"))
                tips += $"• Hedef: Haftada {weight * 0.01:F1}kg yağ kaybı (sağlıklı tempo).\n";

            tips += $"• Dinlenme çok önemli: Haftada en az 1-2 gün tam dinlenme.\n";
            tips += $"• İlerleme takibi: Her hafta aynı gün tartılın ve ölçüm alın.";

            return tips;
        }

        // ===== DİYET OLUŞTURUCU METODLAR =====

        private string GenerateBreakfast(int calories, double protein, string goal)
        {
            if (goal.ToLower().Contains("kas"))
                return $"• 4 yumurta haşlama ({protein:F0}g protein)\n• 1 kase yulaf ezmesi + muz\n• 1 bardak süt\n• 1 avuç fındık";
            else if (goal.ToLower().Contains("kilo"))
                return $"• 2 yumurta omlet ({protein:F0}g protein)\n• 1 dilim tam buğday ekmeği\n• Domates, salatalık\n• 1 bardak yeşil çay";
            else
                return $"• 3 yumurta ({protein:F0}g protein)\n• Avokado + tam buğday tost\n• 1 bardak portakal suyu";
        }

        private string GenerateBreakfast2(int calories, double protein)
        {
            return $"• Protein smoothie (whey + muz + yulaf) ({protein:F0}g protein)\n• 1 avuç badem\n• 1 elma";
        }

        private string GenerateBreakfast3(int calories, double protein)
        {
            return $"• Lor peyniri + domates + zeytin ({protein:F0}g protein)\n• 2 dilim tam buğday ekmeği\n• 1 bardak ayran";
        }

        private string GenerateLunch(int calories, double protein, double carbs, string goal)
        {
            if (goal.ToLower().Contains("kas"))
                return $"• 200g ızgara biftek ({protein:F0}g protein)\n• 1 porsiyon pirinç pilav ({carbs:F0}g karbonhidrat)\n• Bol yeşil salata + zeytinyağı\n• 1 kase çorba";
            else if (goal.ToLower().Contains("kilo"))
                return $"• 150g ızgara tavuk göğsü ({protein:F0}g protein)\n• Bol yeşil salata\n• Yarım porsiyon bulgur\n• Ayran";
            else
                return $"• 180g ızgara somon ({protein:F0}g protein)\n• Tatlı patates (fırında)\n• Buharda brokoli\n• Zeytinyağlı salata";
        }

        private string GenerateLunch2(int calories, double protein, double carbs)
        {
            return $"• 200g izgara hindi göğsü ({protein:F0}g protein)\n• 1 porsiyon kinoa\n• Közlenmiş sebzeler\n• Cacık";
        }

        private string GenerateLunch3(int calories, double protein, double carbs, string goal)
        {
            return $"• Ton balıklı salata (1 kutu ton) ({protein:F0}g protein)\n• Yeşillikler + havuç + mısır\n• {(goal.ToLower().Contains("kilo") ? "Az" : "1 porsiyon")} tam buğday makarna\n• Limonlu sos";
        }

        private string GenerateDinner(int calories, double protein, string goal)
        {
            if (goal.ToLower().Contains("kas"))
                return $"• 200g ızgara tavuk but ({protein:F0}g protein)\n• Sebzeli makarna\n• Yoğurt\n• 1 kase çorba";
            else
                return $"• 180g fırında balık (levrek/çupra) ({protein:F0}g protein)\n• Buharda sebze\n• Yeşil salata\n• Limonlu su";
        }

        private string GenerateDinner2(int calories, double protein)
        {
            return $"• 180g izgara köfte ({protein:F0}g protein)\n• Közlenmiş patlıcan/biber\n• Cacık\n• Yeşil salata";
        }

        private string GenerateDinner3(int calories, double protein)
        {
            return $"• 200g fırında tavuk ({protein:F0}g protein)\n• Fırın sebze (kabak, havuç, patates)\n• Yoğurt\n• Turşu";
        }

        private string GenerateSnacks(int calories, string goal)
        {
            if (goal.ToLower().Contains("kas"))
                return "• Whey protein shake (30g)\n• 1 muz + fıstık ezmesi\n• 1 avuç ceviz";
            else if (goal.ToLower().Contains("kilo"))
                return "• Yoğurt (az yağlı)\n• 10 çiğ badem\n• 1 elma";
            else
                return "• Protein bar\n• 1 muz\n• 1 bardak kefir";
        }

        private string GenerateSnacks2(int calories)
        {
            return "• Humus + havuç çubukları\n• 1 portakal\n• 1 bardak süt";
        }

        private string GenerateSnacks3(int calories)
        {
            return "• Cottage cheese\n• Üzüm (1 kase)\n• 15 fındık";
        }

        private string GenerateDietTips(double calories, double weight, string goal)
        {
            string tips = $"• Her öğünde mutlaka protein tüketin ({calories:F0} kal/gün)\n";
            tips += "• Günde 5-6 öğün şeklinde beslenin\n";

            if (goal.ToLower().Contains("kas"))
                tips += "• Antrenman sonrası 30 dakika içinde protein alın\n";
            else if (goal.ToLower().Contains("kilo"))
                tips += $"• Günlük {calories:F0} kalorinin altına inmeyin (metabolizma yavaşlar)\n";

            tips += "• Haftada 1 gün 'cheat meal' yapabilirsiniz\n";
            tips += $"• Hedef kilo: {(goal.ToLower().Contains("kilo") ? weight - 5 : goal.ToLower().Contains("kas") ? weight + 3 : weight):F1}kg (8-12 hafta içinde)";

            return tips;
        }

        // Resim özellikleri yok
        public Task<string> AnalyzeBodyImageAsync(IFormFile image, string fitnessGoal)
        {
            return Task.FromResult("Resim analizi özelliği şu anda mevcut değil.");
        }

        public Task<string> GenerateFutureBodyImageAsync(string currentDescription, string targetGoal, int timeframe)
        {
            return Task.FromResult("Resim oluşturma özelliği şu anda mevcut değil.");
        }
    }
}