namespace SporSalonuProjesi.Data.Entities
{
    public class Service
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } // Örn: Group Fitness, Yoga [cite: 12]
        public string Description { get; set; }
        public int DurationMinutes { get; set; } // Süre [cite: 12]
        public decimal Fee { get; set; } // Ücret [cite: 12]
        public string ImageUrl { get; set; }
        public virtual ICollection<Trainer> Trainers { get; set; } // Bu hizmeti verebilen antrenörler
    }
}
