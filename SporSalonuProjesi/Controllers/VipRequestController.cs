using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Data.Entities;

namespace SporSalonuProjesi.Controllers
{
    public class VipRequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VipRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Controllers/HomeController.cs

        [HttpPost]
        public async Task<IActionResult> Submit(VipRequest model)
        {
            if (ModelState.IsValid)
            {
                // Formu veritabanına kaydet
                model.SubmittedAt = DateTime.UtcNow; // Tarih damgasını ekle
                await _context.VipRequests.AddAsync(model);
                await _context.SaveChangesAsync();

                // Kullanıcıya başarı mesajı göster
                TempData["VipSuccess"] = "Your VIP request has been sent!";
            }
            // Hangi sayfadan geldiyse (About veya Contact) oraya geri yönlendir
            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }
    }
}
