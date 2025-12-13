namespace SporSalonuProjesi.Data.Entities
{
    public class PricingPlan
    {
        public int Id { get; set; }
        public string Title { get; set; } // Örn: "Yearly", "Monthly"
        public decimal PricePerMonth { get; set; }
        public string Features { get; set; }
    }
}
