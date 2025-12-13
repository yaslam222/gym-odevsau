using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Data.Entities;

namespace SporSalonuProjesi.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context; 
        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View(); // Views/Home/Contact.cshtml'i gösterir
        }

        // YENİ EKLENDİ: /Home/Contact (Formu gönderen POST metodu)
        // 'Send us a message' formunu yakalayan metot
        [HttpPost]
        public async Task<IActionResult> Index(ContactFormMessage model)
        {
            if (ModelState.IsValid)
            {
                // Formu veritabanına kaydet
                await _context.ContactFormMessages.AddAsync(model);
                await _context.SaveChangesAsync();

                // Kullanıcıya başarı mesajı göster
                TempData["ContactSuccess"] = "Your message has been sent to us!";

                // Sayfayı yeniden yükle (formun temizlenmesi için)
                return RedirectToAction("Index");
            }

            // Model geçerli değilse, formu hatalarla birlikte tekrar göster
            return View(model);
        }

    }
}
