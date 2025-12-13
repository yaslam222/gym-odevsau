using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Models;
using System.Diagnostics;

namespace SporSalonuProjesi.Controllers
{
    public class HomeController : Controller
    {
      
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult>Index()
        {
            // 1. Yeni ViewModel oluştur
            var viewModel = new HomeViewModel
            {
                // 2. Fiyat Planlarını veritabanından çek (İlk 3 tanesini al)
                PricingPlans = await _context.PricingPlans.Take(3).ToListAsync(),

                // 3. Dersleri veritabanından çek (İlk 4 tanesini al)
                GymClasses = await _context.GymClasses.Take(4).ToListAsync()
            };

            // 4. İki listeyi de View'a (Index.cshtml) gönder
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
