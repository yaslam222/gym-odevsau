using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;

namespace SporSalonuProjesi.Controllers
{
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Burası /Staff veya /Staff/Index adresini çalıştırır
        // ve veritabanındaki TÜM eğitmenleri listeler
        public async Task<IActionResult> Index()
        {
            var trainers = await _context.Trainers.ToListAsync();
            // trainers listesini View'a (Index.cshtml) gönderir
            return View(trainers);
        }

        // Burası /Staff/Detail/1 gibi adresleri çalıştırır
        // "View More" butonunun hedefi budur
        public async Task<IActionResult> Detail(int id)
        {
            // Tek bir eğitmeni, ID'sine göre Müsaitlik (Availabilities) bilgisiyle birlikte bul
            var trainer = await _context.Trainers
                .Include(t => t.Availabilities)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
            {
                return NotFound(); // Eğitmen bulunamazsa 404 hatası ver
            }

            // 'trainer' modelini View'a (Detail.cshtml) gönder
            return View(trainer);
        }
    }
}
