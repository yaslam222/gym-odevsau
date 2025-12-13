using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;

namespace SporSalonuProjesi.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("appointments-by-date")]
        public async Task<IActionResult> GetAppointmentsByDate([FromQuery] DateTime date)
        {
            // LINQ: 'Where' kullanarak filtreleme yapıyoruz
            var appointments = await _context.Appointments
               .Where(a => a.AppointmentDateTime.Date == date.Date)
               .Include(a => a.Trainer)  // İlişkili antrenör verisi
               .Include(a => a.GymClass) // İlişkili ders verisi
               .Select(a => new {
                   a.Id,
                   a.AppointmentDateTime,
                   // DÜZELTME: 'IsApproved' (bool) yerine 'Status' (enum) kullanıyoruz
                   Status = a.Status.ToString(), // Bu, "Pending", "Approved", "Rejected" metnini döndürür
                   TrainerName = a.Trainer.FullName,
                   ClassName = a.GymClass.Name,
                   a.Fee
               })
               .ToListAsync();

            if (!appointments.Any())
            {
                // 404 Not Found durum kodu döndür
                return NotFound(new { message = "Belirtilen tarih için randevu bulunamadı." });
            }

            return Ok(appointments); // 200 OK ile JSON verisi
        }
    }
}
