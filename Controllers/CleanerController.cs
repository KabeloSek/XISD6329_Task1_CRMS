using Microsoft.AspNetCore.Mvc;
using XISD6329_Task1_CRMS.Models;

namespace XISD6329_Task1_CRMS.Controllers
{
    public class CleanerController : Controller
    {
        public IActionResult CleanerHome()
        {
            if (HttpContext.Session.GetInt32("CleanerID") == null)
                return RedirectToAction("LoginCleaner", "Home");

            return View();
        }

        public IActionResult CleanerRequests()
        {
            if (HttpContext.Session.GetInt32("CleanerID") == null)
                return RedirectToAction("LoginCleaner", "Home");

            CleanerModel model = new CleanerModel();
            return View(model.GetOpenRequests());
        }

        [HttpPost]
        public IActionResult AcceptBooking(int bookingId)
        {
            int? cleanerId = HttpContext.Session.GetInt32("CleanerID");
            string cleanerName = HttpContext.Session.GetString("CleanerName");
            if (cleanerId == null) return RedirectToAction("LoginCleaner", "Home");

            CleanerModel model = new CleanerModel();
            model.AcceptBooking(bookingId, cleanerId.Value, cleanerName);

            return RedirectToAction("CleanerRequests");
        }

        public IActionResult CleanerBookings()
        {
            int? cleanerId = HttpContext.Session.GetInt32("CleanerID");
            if (cleanerId == null) return RedirectToAction("LoginCleaner", "Home");

            CleanerModel model = new CleanerModel();
            return View(model.GetCleanerBookings(cleanerId.Value));
        }

        [HttpPost]
        public IActionResult CompleteBooking(int bookingId, string passkey)
        {
            int? cleanerId = HttpContext.Session.GetInt32("CleanerID");
            string cleanerName = HttpContext.Session.GetString("CleanerName");
            if (cleanerId == null) return RedirectToAction("LoginCleaner", "Home");

            CleanerModel model = new CleanerModel();
            bool success = model.CompleteBooking(bookingId, passkey, cleanerName);

            if (!success)
            {
                TempData["Error"] = "Incorrect passkey — ask the student to confirm it.";
            }

            return RedirectToAction("CleanerBookings");
        }

        [HttpGet]
        public IActionResult CleanerProfile()
        {
            string currentEmail = HttpContext.Session.GetString("CleanerEmail");
            if (string.IsNullOrEmpty(currentEmail))
            {
                return RedirectToAction("LoginCleaner", "Home");
            }

            CleanerProfileModel profile = new CleanerProfileModel();
            CleanerProfileModel cleaner = profile.Get_Cleaner(currentEmail);

            return View(cleaner);
        }

        [HttpPost]
        public IActionResult CleanerProfile(CleanerProfileModel updated)
        {
            string currentEmail = HttpContext.Session.GetString("CleanerEmail");
            if (string.IsNullOrEmpty(currentEmail))
            {
                return RedirectToAction("LoginCleaner", "Home");
            }

            if (ModelState.IsValid)
            {
                CleanerProfileModel profile = new CleanerProfileModel();
                bool success = profile.Update_Cleaner(currentEmail, updated.email, updated.newPassword);

                if (success)
                {
                    HttpContext.Session.SetString("CleanerEmail", updated.email);
                    ViewBag.Message = "Profile updated successfully.";
                }
                else
                {
                    ViewBag.Message = "Update failed. That email may already be in use.";
                }
            }

            CleanerProfileModel refreshed = new CleanerProfileModel().Get_Cleaner(HttpContext.Session.GetString("CleanerEmail"));
            return View(refreshed);
        }
    }
}