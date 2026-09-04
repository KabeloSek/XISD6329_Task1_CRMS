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
        public IActionResult ReportProblem() 
        { 
            return View();
        } 
        [HttpPost]
        [HttpPost]
        public IActionResult CleaningGuidelines()
        {
            return View();
        }

        public IActionResult ContactResidenceManagement()
        {
            return View();
        }
        public IActionResult ReportProblem(string Issue, string Room, IFormFile Photo)
        {
            if (string.IsNullOrWhiteSpace(Issue))
            {
                ModelState.AddModelError("Issue", "Please describe the problem.");
            }

            if (string.IsNullOrWhiteSpace(Room))
            {
                ModelState.AddModelError("Room", "Please select a room number.");
            }

            if (Photo == null || Photo.Length == 0)
            {
                ModelState.AddModelError("Photo", "Please upload a photo.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            TempData["SuccessMessage"] = "Report successfully submitted.";

            return RedirectToAction("ReportProblem");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}
