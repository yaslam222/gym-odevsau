using System.ComponentModel.DataAnnotations;

namespace SporSalonuProjesi.Data.Entities
{
    public class TrainerAvailability
    {
        public int Id { get; set; }
        public DayOfWeek Day { get; set; } // Haftanın Günü (Pazartesi, Salı vb.)
        public TimeSpan StartTime { get; set; } // Başlangıç Saati (Örn: 09:00)
        public TimeSpan EndTime { get; set; } // Bitiş Saati (Örn: 17:00)

        // İlişki (Hangi antrenöre ait?)
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }
    }
}
