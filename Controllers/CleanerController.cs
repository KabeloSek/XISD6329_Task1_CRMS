using Microsoft.AspNetCore.Mvc;

namespace XISD6329_Task1_CRMS.Controllers
{
    public class CleanerController : Controller
    {
        public IActionResult CleanerHome()
        {
            return View();
        }
    }
}
