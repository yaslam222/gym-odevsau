namespace SporSalonuProjesi.Data.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime AppointmentDateTime { get; set; } // Randevunun tarihi ve saati
        public int DurationMinutes { get; set; } // Randevu anındaki süre
        public decimal Fee { get; set; } // Randevu anındaki ücret

        // --- PDF'ten Gelen Alanlar ---
        public BookingStatus Status { get; set; } // Randevu onay mekanizması

        // İlişkiler (Bu randevu kime ait?)
        public string MemberId { get; set; } // Hangi üye aldı?
        public ApplicationUser Member { get; set; }

        public int TrainerId { get; set; } // Hangi antrenörden?
        public Trainer Trainer { get; set; }

        public int GymClassId { get; set; } // Hangi ders için?
        public GymClass GymClass { get; set; }
    }
}
