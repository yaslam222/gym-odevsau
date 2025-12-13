using Microsoft.AspNetCore.Identity;

namespace SporSalonuProjesi.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        // --- PDF (Yapay Zeka) için Gerekli Olabilecek Alanlar ---
        public double? Height { get; set; } // Boy (cm)
        public double? Weight { get; set; } // Kilo (kg)
        public string ?Goals { get; set; } // Hedefler (Örn: "Kilo vermek", "Kas yapmak")

        // İlişki (Üyenin randevuları)
        public ICollection<Appointment> Appointments { get; set; }
    }
}