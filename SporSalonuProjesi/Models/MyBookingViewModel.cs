using SporSalonuProjesi.Data.Entities;

namespace SporSalonuProjesi.Models
{
    public class MyBookingViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string TrainerName { get; set; }
        public string ClassName { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Fee { get; set; }
        public BookingStatus Status { get; set; }
    }
}
