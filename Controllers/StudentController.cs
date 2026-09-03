using Microsoft.AspNetCore.Mvc;

namespace XISD6329_Task1_CRMS.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult StudentHome()
        {
            string studentName = HttpContext.Session.GetString("StudentName");

            //if there's no session, they never logged in — send them back
            if (string.IsNullOrEmpty(studentName))
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.StudentName = studentName;
            return View();
        }
        public IActionResult WhoCleaningIsFor()
        {
            return View();
        }
        public IActionResult RequestCleaningForm()
        {
            return View();
        }
        public IActionResult RequestForElse()
        {
            return View();
        }
        public IActionResult MyBookings()
        {
            return View();
        }
        public IActionResult StudentProfile()
        {
            return View();
        }
        public IActionResult StudentHelpSupport()
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}
