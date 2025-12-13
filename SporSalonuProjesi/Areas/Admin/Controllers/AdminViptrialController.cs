using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SporSalonuProjesi.Data;

namespace SporSalonuProjesi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminViptrialController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public AdminViptrialController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var viptrials = dbContext.VipRequests.ToList();
            return View(viptrials);
        }
        public IActionResult Delete(int id)
        {
            var viptrial = dbContext.VipRequests.Find(id);
            if (viptrial != null)
            {
                dbContext.VipRequests.Remove(viptrial);
                dbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
