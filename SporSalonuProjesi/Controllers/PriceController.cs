using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;

namespace SporSalonuProjesi.Controllers
{
    public class PriceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PriceController(ApplicationDbContext context)
        {
            _context = context;
        }
       
        public async Task<IActionResult> Index()
        {
            // Veritabanındaki 'PricingPlans' tablosundan tüm planları al
            var plans = await _context.PricingPlans.ToListAsync();

            // 'plans' listesini View'a (Pricing.cshtml) gönder
            return View(plans);
        }
    }
}
