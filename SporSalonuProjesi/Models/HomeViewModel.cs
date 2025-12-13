using SporSalonuProjesi.Data.Entities;

namespace SporSalonuProjesi.Models
{
    public class HomeViewModel
    {
        public List<PricingPlan> PricingPlans { get; set; }
        public List<GymClass> GymClasses { get; set; }
    }
}
