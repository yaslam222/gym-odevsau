namespace SporSalonuProjesi.Data.Entities
{
    public class Trainer
    {
        public int Id { get; set; }
        public string FullName { get; set; } // Örn: "Jessica Doe"
        public string Title { get; set; } // Örn: "Personal Trainer"
        public string Description { get; set; } // Örn: "Personal Trainer"
        public string ProfileImageUrl { get; set; } // Örn: "img/demos/gym/staff/staff-1.jpg"

        // --- demo-gym-staff-detail.html sayfasından gelen alanlar ---
        public string Bio { get; set; } // Uzun biyografi
        public string Skills { get; set; } // "Fitness,Zumba,Cardio" gibi (veya ayrı bir tablo yapılabilir)

        // --- PDF'ten Gelen Alanlar ---
        public string Speciality { get; set; } // Uzmanlık Alanı (Örn: "Kilo Verme", "Kas Geliştirme")

        // İlişkiler
        public ICollection<TrainerAvailability>? Availabilities { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
