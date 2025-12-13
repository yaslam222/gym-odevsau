using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için
using Microsoft.EntityFrameworkCore; // Include için
using SporSalonuProjesi.Data; // DbContext için
using SporSalonuProjesi.Data.Entities; // TrainerAvailability için

namespace SporSalonuProjesi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminAvailableController : Controller
    {
        private readonly ApplicationDbContext _context;

        // 2. DbContext'i inject edin
        public AdminAvailableController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 3. INDEX (Listeleme) metodu
       
            // Müsaitlikleri, ilgili Antrenör bilgisiyle birlikte çek
         public IActionResult Index()
        {
            var groupedAvailabilities = _context.TrainerAvailabilities
                                         .Include(t => t.Trainer)
                                         .OrderBy(t => t.Trainer.FullName) // Önce isme göre sırala
                                         .ThenBy(t => t.Day)              // Sonra güne göre sırala
                                         .AsEnumerable()                  // Gruplamayı bellekte yapmak için
                                         .GroupBy(t => t.Trainer);       // Antrenöre göre grupla

            // Gruplanmış modeli View'a gönder
            return View(groupedAvailabilities);
        }
       
        // 4. CREATE (Yeni Ekleme - Formu Gösterme)
        [HttpGet]
        public IActionResult Create()
        {
            // Forma Antrenörleri listeleyen bir dropdown göndermeliyiz
            ViewBag.Trainers = new SelectList(_context.Trainers, "Id", "FullName");
            return View();
        }

        // 5. CREATE (Yeni Ekleme - Formu Kaydetme)
        [HttpPost]
        public IActionResult Create(TrainerAvailability availability)
        {
            // Basit bir model doğrulaması
            if (availability.TrainerId > 0 && availability.EndTime > availability.StartTime)
            {
                _context.TrainerAvailabilities.Add(availability);
                _context.SaveChanges();
                return RedirectToAction("Index"); // Listeye geri dön
            }

            // Hata varsa formu tekrar göster
            ViewBag.Trainers = new SelectList(_context.Trainers, "Id", "FullName", availability.TrainerId);
            return View(availability);
        }

        // 6. DELETE (Silme) metodu
        public IActionResult Delete(int id)
        {
            var availability = _context.TrainerAvailabilities.Find(id);
            if (availability != null)
            {
                _context.TrainerAvailabilities.Remove(availability);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    

        // --- YENİ METOT: DÜZENLE (EDIT - Formu Gösterme) ---
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var availability = _context.TrainerAvailabilities.Find(id);
            if (availability == null)
            {
                return NotFound();
            }

            // 'Create' metodundaki gibi, Trainer dropdown'ı için
            // antrenör listesini View'a göndermeliyiz.
            ViewBag.Trainers = new SelectList(_context.Trainers, "Id", "FullName", availability.TrainerId);
            return View(availability);
        }

        // --- YENİ METOT: DÜZENLE (EDIT - Formu Kaydetme) ---
        [HttpPost]
        public IActionResult Edit(TrainerAvailability availability)
        {
            // 'Create' metodunuzdaki gibi basit bir doğrulama:
            if (availability.TrainerId > 0 && availability.EndTime > availability.StartTime)
            {
                _context.TrainerAvailabilities.Update(availability);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Hata varsa, formu tekrar göster
            // Dropdown'ı yeniden doldurmayı unutmayın!
            ViewBag.Trainers = new SelectList(_context.Trainers, "Id", "FullName", availability.TrainerId);
            return View(availability);
        }
    }
}
    
