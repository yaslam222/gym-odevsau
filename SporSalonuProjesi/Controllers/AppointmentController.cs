using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Data.Entities; // <-- BookingStatus enum'u için GEREKLİ
using SporSalonuProjesi.Models;
using System.Security.Claims;
using System.Linq; // .Where() ve .Any() için GEREKLİ

namespace SporSalonuProjesi.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //
        // GET: /Appointment/Index
        // (Randevu alma sayfasını yükler)
        //
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // Identity'nin login sayfasına yönlendir
                return Redirect("/Identity/Account/Login");
            }
            var viewModel = new BookingPageViewModel
            {
                Trainers = await _context.Trainers.OrderBy(t => t.FullName).ToListAsync(),
                GymClasses = await _context.GymClasses.OrderBy(g => g.Name).ToListAsync()
            };

            return View(viewModel);
        }

        //
        // POST: /Appointment/CreateAppointment
        // (AJAX ile yeni randevu talebi oluşturur)
        //
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "You must log in." });
            }

            var gymClass = await _context.GymClasses.FindAsync(model.GymClassId);
            var trainer = await _context.Trainers.FindAsync(model.TrainerId);

            if (gymClass == null || trainer == null)
            {
                return Json(new { success = false, message = "Invalid Trainer or Service." });
            }

            // --- GÜVENLİK KONTROLÜ (BookingStatus enum'u ile güncellendi) ---
            DateTime potentialSlotStart = model.AppointmentDateTime;
            DateTime potentialSlotEnd = potentialSlotStart.AddMinutes(gymClass.DurationMinutes);

            // 1. Bu kullanıcının bu saatte zaten bir talebi var mı?
            var existingRequestForUser = await _context.Appointments
                .FirstOrDefaultAsync(a => a.MemberId == userId &&
                                          a.AppointmentDateTime == potentialSlotStart);

            if (existingRequestForUser != null)
            {
                if (existingRequestForUser.Status == BookingStatus.Approved)
                {
                    return Json(new { success = false, message = "You already have an APPROVED appointment for this time." });
                }
                else if (existingRequestForUser.Status == BookingStatus.Pending)
                {
                    return Json(new { success = false, message = "You already have a PENDING request for this time." });
                }
                else // Rejected
                {
                    return Json(new { success = false, message = "Your previous request for this time was rejected. Please book a different time or contact support." });
                }
            }

            // 2. Bu saat başka bir kullanıcı tarafından (Onaylı veya Beklemede) dolu mu?
            var trainerConflicts = await _context.Appointments
                .Where(a => a.TrainerId == model.TrainerId &&
                             (a.Status == BookingStatus.Approved || a.Status == BookingStatus.Pending)) // Reddedilenler çakışma sayılmaz
                .ToListAsync();

            foreach (var booking in trainerConflicts)
            {
                DateTime bookingStart = booking.AppointmentDateTime;
                DateTime bookingEnd = booking.AppointmentDateTime.AddMinutes(booking.DurationMinutes);

                if (potentialSlotStart < bookingEnd && potentialSlotEnd > bookingStart)
                {
                    // Saat başkası tarafından dolmuş
                    return Json(new { success = false, message = "Sorry, this slot was just taken. Please refresh and select a new time." });
                }
            }
            // --- GÜVENLİK KONTROLÜ SONU ---

            var newAppointment = new Appointment
            {
                MemberId = userId,
                TrainerId = model.TrainerId,
                GymClassId = model.GymClassId,
                AppointmentDateTime = model.AppointmentDateTime,
                DurationMinutes = gymClass.DurationMinutes,
                Fee = gymClass.Fee,
                Status = BookingStatus.Pending // Yeni randevular 'Bekliyor' olarak başlar
            };

            _context.Appointments.Add(newAppointment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Your appointment request has been successfully submitted. Waiting for approval." });
        }

        //
        // GET: /Appointment/GetTrainerWorkDays
        // (Dinamik takvim için antrenörün çalışma günlerini (Pzt=1, Salı=2 vb.) döndürür)
        //
        [HttpGet]
        public async Task<IActionResult> GetTrainerWorkDays(int trainerId)
        {
            var workDays = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainerId)
                .Select(a => (int)a.Day) // DayOfWeek enum'unu int'e çevirir (Pazar=0, Pzt=1...)
                .ToListAsync();

            return Json(workDays);
        }

        //
        // GET: /Appointment/GetAvailableSlots
        // (AJAX ile seçilen güne göre müsait saatleri listeler)
        //
        [HttpGet]
        public async Task<JsonResult> GetAvailableSlots(int trainerId, int gymClassId, DateTime date)
        {
            try
            {
                var gymClass = await _context.GymClasses.FindAsync(gymClassId);
                if (gymClass == null) { return Json(new List<string>()); }
                int duration = gymClass.DurationMinutes;

                // 1. Antrenörün o günkü çalışma programını bul
                DayOfWeek selectedDay = date.DayOfWeek;
                var availability = await _context.TrainerAvailabilities
                    .FirstOrDefaultAsync(a => a.TrainerId == trainerId && a.Day == selectedDay);

                // 2. O GÜN ÇALIŞMIYORSA (İstediğiniz Hata Mesajı Mantığı)
                if (availability == null)
                {
                    // Antrenörün diğer tüm çalışma günlerini bul
                    var allWorkDays = await _context.TrainerAvailabilities
                        .Where(a => a.TrainerId == trainerId)
                        .OrderBy(a => a.Day)
                        .Select(a => a.Day.ToString()) // "Monday", "Tuesday" vb.
                        .ToListAsync();

                    string availableDaysString = "No schedule found for this trainer.";
                    if (allWorkDays.Any())
                    {
                        availableDaysString = string.Join(", ", allWorkDays);
                    }

                    // Hata mesajını ve müsait günleri birlikte döndür
                    return Json(new
                    {
                        error = "This trainer is not available on this day.",
                        availableDays = availableDaysString
                    });
                }

                // 3. O GÜN ÇALIŞIYORSA (Müsait saatleri hesapla)

                // O günkü dolu saatleri (Onaylı veya Beklemede) bul
                var existingBookings = await _context.Appointments
                    .Where(a => a.TrainerId == trainerId &&
                                 a.AppointmentDateTime.Date == date.Date &&
                                 (a.Status == BookingStatus.Approved || a.Status == BookingStatus.Pending))
                    .ToListAsync();

                var availableSlots = new List<string>();

                // Antrenörün programına göre başlangıç ve bitiş saatlerini ayarla
                DateTime dayStart = date.Date.Add(availability.StartTime);
                DateTime dayEnd = date.Date.Add(availability.EndTime);
                DateTime potentialSlotStart = dayStart;

                while (potentialSlotStart.AddMinutes(duration) <= dayEnd)
                {
                    bool hasConflict = false;
                    DateTime potentialSlotEnd = potentialSlotStart.AddMinutes(duration);

                    foreach (var booking in existingBookings)
                    {
                        DateTime bookingStart = booking.AppointmentDateTime;
                        DateTime bookingEnd = booking.AppointmentDateTime.AddMinutes(booking.DurationMinutes);

                        if (potentialSlotStart < bookingEnd && potentialSlotEnd > bookingStart)
                        {
                            hasConflict = true;
                            break;
                        }
                    }

                    if (!hasConflict)
                    {
                        availableSlots.Add(potentialSlotStart.ToString("HH:mm"));
                    }

                    potentialSlotStart = potentialSlotStart.AddMinutes(duration);
                }

                return Json(availableSlots);
            }
            catch (Exception ex)
            {
                // Bir hata olursa logla (opsiyonel) ve hata döndür
                return Json(new { error = "An error occurred while loading slots." });
            }
        }

        //
        // GET: /Appointment/MyBookings
        // (Kullanıcının kendi randevularını gösterir)
        //
        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookings = await _context.Appointments
                .Where(a => a.MemberId == userId)
                .Include(a => a.Trainer)
                .Include(a => a.GymClass)
                .OrderByDescending(a => a.AppointmentDateTime)
                .Select(a => new MyBookingViewModel
                {
                    Id = a.Id,
                    AppointmentDateTime = a.AppointmentDateTime,
                    TrainerName = a.Trainer.FullName,
                    ClassName = a.GymClass.Name,
                    DurationMinutes = a.DurationMinutes,
                    Fee = a.Fee,
                    Status = a.Status // 'IsApproved' yerine 'Status'
                })
                .ToListAsync();

            return View(bookings);
        }
    }
}