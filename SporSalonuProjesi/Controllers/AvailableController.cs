using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Data.Entities;

namespace SporSalonuProjesi.Controllers
{
    public class AvailableController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AvailableController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<JsonResult> GetAvailableSlots(int trainerId, int gymClassId, DateTime date)
        {
            try
            {
                // 1. Seçilen dersin süresini al
                var gymClass = await _context.GymClasses.FindAsync(gymClassId);
                if (gymClass == null)
                {
                    return Json(new List<string>()); // Hizmet bulunamazsa boş dön
                }
                int duration = gymClass.DurationMinutes;

                // 2. O antrenörün o günkü MEVCUT randevularını veritabanından al
                //    (Include ile ders süresini de alıyoruz ki çakışmayı doğru hesaplayalım)
                var existingBookings = await _context.Appointments
     .Include(a => a.GymClass) // Her randevunun ders bilgisini de çek
     .Where(a => a.TrainerId == trainerId &&
                  a.AppointmentDateTime.Date == date.Date &&
                  (a.Status == BookingStatus.Approved || a.Status == BookingStatus.Pending)) // DÜZELTME: Bir saat, hem onaylanmışsa hem de onay bekliyorsa DOLU sayılır.
     .ToListAsync();

                // 3. Potansiyel slotları oluştur (Örn: 09:00 - 17:00 arası)
                var availableSlots = new List<string>();

                // Bu saatleri veritabanından (Trainer.WorkingHours) çekmek en doğrusu olur.
                // Şimdilik 09:00 - 17:00 arası varsayalım:
                DateTime dayStart = date.Date.AddHours(9);  // Sabah 09:00
                DateTime dayEnd = date.Date.AddHours(17);   // Akşam 17:00

                DateTime potentialSlotStart = dayStart;

                // 4. Potansiyel slotları mevcut randevularla karşılaştırarak filtrele
                while (potentialSlotStart.AddMinutes(duration) <= dayEnd)
                {
                    bool hasConflict = false;
                    DateTime potentialSlotEnd = potentialSlotStart.AddMinutes(duration);

                    // Mevcut randevularla çakışma var mı?
                    foreach (var booking in existingBookings)
                    {
                        DateTime bookingStart = booking.AppointmentDateTime;
                        DateTime bookingEnd = booking.AppointmentDateTime.AddMinutes(booking.GymClass.DurationMinutes);

                        // Çakışma Mantığı: (BaşlangıçA < BitişB) && (BitişA > BaşlangıçB)
                        if (potentialSlotStart < bookingEnd && potentialSlotEnd > bookingStart)
                        {
                            hasConflict = true;
                            break; // Çakışma bulundu, bu slotu es geç
                        }
                    }

                    // 5. Çakışma Yoksa: Bu slot müsaittir
                    if (!hasConflict)
                    {
                        availableSlots.Add(potentialSlotStart.ToString("HH:mm"));
                    }

                    // Bir sonraki potansiyel slot'a geç
                    // ÖNEMLİ: Slotları ders süresine göre değil, 
                    // 30dk gibi sabit aralıklarla (veya ders süresiyle) artırabilirsiniz.
                    // Şimdilik ders süresi kadar artıralım:
                    potentialSlotStart = potentialSlotStart.AddMinutes(duration);
                    // Daha iyi bir yaklaşım: potentialSlotStart = potentialSlotStart.AddMinutes(30); // 30dk'lık slotlar
                }

                return Json(availableSlots);
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                return Json(new { error = "Slotlar yüklenirken bir hata oluştu." });
            }
        }
    }
}
