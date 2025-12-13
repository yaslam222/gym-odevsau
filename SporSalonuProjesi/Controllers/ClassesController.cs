using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;
using System.Threading.Tasks;

namespace SporSalonuProjesi.Controllers
{
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Burası /Classes veya /Classes/Index adresini çalıştırır
        // ve veritabanındaki TÜM dersleri listeler
        public async Task<IActionResult> Index()
        {
            // Veritabanındaki GymClasses tablosundaki tüm verileri al
            var classes = await _context.GymClasses.ToListAsync();

            // 'classes' listesini View'a (Index.cshtml) gönder
            return View(classes);
        }

        // Burası /Classes/Detail/1 gibi adresleri çalıştırır
        // "Learn More" butonunun hedefi budur
        public async Task<IActionResult> Detail(int id)
        {
            // ID'si eşleşen tek bir dersi veritabanından bul
            var gymClass = await _context.GymClasses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (gymClass == null)
            {
                return NotFound(); // Ders bulunamazsa 404 hatası ver
            }

            // 'gymClass' modelini View'a (Detail.cshtml) gönder
            return View(gymClass);
        }
    }
}