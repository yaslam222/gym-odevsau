using Microsoft.AspNetCore.Mvc;

namespace SporSalonuProjesi.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
