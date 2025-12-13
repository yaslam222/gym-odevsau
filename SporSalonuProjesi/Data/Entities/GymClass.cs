namespace SporSalonuProjesi.Data.Entities
{
    public class GymClass
    {
        public int Id { get; set; }
        public string Name { get; set; } // Örn: "Group Fitness", "Yoga"
        public string Title { get; set; } // Örn: "Group Fitness", "Yoga"
        public string Description { get; set; }
        public string ImageUrl { get; set; } // Örn: "img/demos/gym/classes/classes-1.jpg"

        // --- PDF'ten Gelen Alanlar ---
        public int DurationMinutes { get; set; } // Hizmetin süresi (dakika)
        public decimal Fee { get; set; } // Hizmetin ücreti
    }
}
