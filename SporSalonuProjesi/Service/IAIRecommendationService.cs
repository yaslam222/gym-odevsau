namespace SporSalonuProjesi.Service
{
    public interface IAIRecommendationService
    {
        Task<string> GetWorkoutRecommendationAsync(double height, double weight, string bodyType, string fitnessGoal);
        Task<string> GetDietPlanAsync(double height, double weight, string bodyType, string fitnessGoal);
        Task<string> AnalyzeBodyImageAsync(IFormFile image, string fitnessGoal);
        Task<string> GenerateFutureBodyImageAsync(string currentDescription, string targetGoal, int timeframe);
    }
}
