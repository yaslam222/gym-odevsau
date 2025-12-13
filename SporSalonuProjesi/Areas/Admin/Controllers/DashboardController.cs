using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SporSalonuProjesi.Areas.Admin.Models;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Data.Entities;

namespace SporSalonuProjesi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Veritabanına ve Üye yöneticisine erişim için inject ediyoruz
        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // 1. ViewModel'den boş bir nesne oluştur
            var viewModel = new DashboardViewModel();

            // 2. Veritabanından tüm sayıları çek ve doldur
            viewModel.TrainerCount = _context.Trainers.Count();
            viewModel.ClassCount = _context.GymClasses.Count();
            viewModel.MessageCount = _context.ContactFormMessages.Count();
            viewModel.TotalAppointmentCount = _context.Appointments.Count();

            // Onaylananları filtreleyerek say
            viewModel.ApprovedAppointmentCount = _context.Appointments
                                                     .Count(a => a.Status == BookingStatus.Approved);

            // Toplam üye sayısı (Identity sisteminden)
            viewModel.MemberCount = _userManager.Users.Count();

            // 3. Doldurduğun modeli View'a gönder
            return View(viewModel);
        }
    }
}