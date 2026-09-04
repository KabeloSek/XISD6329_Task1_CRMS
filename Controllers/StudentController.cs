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
        [HttpGet]
        public IActionResult RequestCleaningForm()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentID");
            if (studentId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            CleaningRequestModel model = new CleaningRequestModel
            {
                roomNumber = HttpContext.Session.GetString("StudentRoom")
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult RequestCleaningForm(CleaningRequestModel booking)
        {
            int? studentId = HttpContext.Session.GetInt32("StudentID");
            if (studentId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (ModelState.IsValid)
            {
                CleaningRequestModel request = new CleaningRequestModel();
                request.StoreBooking(studentId.Value, booking.roomNumber, booking.bookingDate, booking.roomType, booking.timeSlot, booking.cleaningType, booking.specialInstructions);

                return RedirectToAction("MyBookings", "Student");
            }

            return View(booking);
        }
        [HttpGet]
        public IActionResult RequestForElse()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentID");
            if (studentId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            return View(new CleaningRequestModel());
        }

        [HttpPost]
        public IActionResult RequestForElse(CleaningRequestModel booking)
        {
            int? loggedInStudentId = HttpContext.Session.GetInt32("StudentID");
            if (loggedInStudentId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (ModelState.IsValid)
            {
                CleaningRequestModel request = new CleaningRequestModel();

                //validate the External Booking ID actually belongs to a real student
                int? targetStudentId = request.FindStudentByExternalId(booking.externalBookingId);

                if (targetStudentId == null)
                {
                    ModelState.AddModelError("externalBookingId", "That External Booking ID doesn't match any student.");
                    return View(booking);
                }

                request.StoreBooking(targetStudentId.Value, booking.roomNumber, booking.bookingDate, booking.roomType, booking.timeSlot, booking.cleaningType, booking.specialInstructions);
                return RedirectToAction("MyBookings", "Student");
            }

            return View(booking);
        }
        [HttpGet]
        public IActionResult MyBookings()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentID");
            if (studentId == null) return RedirectToAction("Login", "Home");

            CleanerModel model = new CleanerModel();
            List<BookingViewModel> bookings = model.GetStudentBookings(studentId.Value);
            return View(bookings);
        }
        [HttpGet]
        public IActionResult StudentNotifications()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentID");
            if (studentId == null) return RedirectToAction("Login", "Home");

            NotificationModel model = new NotificationModel();
            List<NotificationViewModel> notifications = model.GetNotifications(studentId.Value);
            return View(notifications);
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
