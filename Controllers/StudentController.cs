using Microsoft.AspNetCore.Mvc;
using XISD6329_Task1_CRMS.Models;

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
        [HttpGet]
        public IActionResult StudentProfile()
        {
            string currentEmail = HttpContext.Session.GetString("StudentEmail");

            if (string.IsNullOrEmpty(currentEmail))
            {
                return RedirectToAction("Login", "Home");
            }

            ProfileModel profile = new ProfileModel();
            ProfileModel student = profile.Get_Student(currentEmail);

            return View(student);
        }

        [HttpPost]
        public IActionResult StudentProfile(ProfileModel updated)
        {
            string currentEmail = HttpContext.Session.GetString("StudentEmail");

            if (string.IsNullOrEmpty(currentEmail))
            {
                return RedirectToAction("Login", "Home");
            }

            if (ModelState.IsValid)
            {
                ProfileModel profile = new ProfileModel();
                bool success = profile.Update_Student(currentEmail, updated.email, updated.newPassword);

                if (success)
                {
                    //keep session in sync with the (possibly changed) email
                    HttpContext.Session.SetString("StudentEmail", updated.email);
                    ViewBag.Message = "Profile updated successfully.";
                }
                else
                {
                    ViewBag.Message = "Update failed. That email may already be in use.";
                }
            }

            //re-fetch to show the current state either way
            ProfileModel refreshed = new ProfileModel().Get_Student(HttpContext.Session.GetString("StudentEmail"));
            return View(refreshed);
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
