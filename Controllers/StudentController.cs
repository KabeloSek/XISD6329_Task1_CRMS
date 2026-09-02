using Microsoft.AspNetCore.Mvc;

namespace XISD6329_Task1_CRMS.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult StudentHome()
        {
            return View();
        }
        public IActionResult RequestCleaning()
        {
            return View();
        }
    }
}
