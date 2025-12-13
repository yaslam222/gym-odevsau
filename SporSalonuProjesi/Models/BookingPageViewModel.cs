using SporSalonuProjesi.Data.Entities;

namespace SporSalonuProjesi.Models
{
    public class BookingPageViewModel
    {// Antrenör listesi dropdown'ı için
        public IEnumerable<Trainer> Trainers { get; set; }

        // Hizmet/Ders listesi dropdown'ı için
        public IEnumerable<GymClass> GymClasses { get; set; }
    }
}
