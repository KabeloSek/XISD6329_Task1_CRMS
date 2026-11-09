using Microsoft.AspNetCore.Mvc;
using XISD6329_Task1_CRMS.Models;
using XISD6329_Task1_CRMS.Services;

namespace XISD6329_Task1_CRMS.Controllers
{
    public class StudentController : Controller
    {
        private readonly FirebaseService firebase;

        public StudentController(FirebaseService firebase)
        {
            this.firebase = firebase;
        }

        public IActionResult StudentHome()
        {
            //if there's no session, they never logged in — send them back
            if (!firebase.IsStudent)
                return RedirectToAction("Login", "Home");

            ViewBag.StudentName = firebase.CurrentName;
            return View();
        }

        public IActionResult WhoCleaningIsFor()
        {
            if (!firebase.IsStudent) return RedirectToAction("Login", "Home");
            return View();
        }

        // ------------------------------------------------------------ request cleaning: for myself

        [HttpGet]
        public IActionResult RequestCleaningForm()
        {
            if (!firebase.IsStudent) return RedirectToAction("Login", "Home");

            return View(new CleaningRequestModel { roomNumber = firebase.CurrentRoom });
        }

        [HttpPost]
        public async Task<IActionResult> RequestCleaningForm(CleaningRequestModel booking)
        {
            if (!firebase.IsStudent) return RedirectToAction("Login", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    await firebase.CreateBooking(firebase.CurrentUid!, false, firebase.CurrentRoom, booking.bookingDate,
                        booking.roomType, booking.timeSlot, booking.cleaningType, booking.specialInstructions);
                    return RedirectToAction("MyBookings", "Student");
                }
                catch (FirebaseException error)
                {
                    ModelState.AddModelError("", error.Message);
                }
            }

            return View(booking);
        }

        // ------------------------------------------------------------ request cleaning: for someone else

        [HttpGet]
        public IActionResult RequestForElse()
        {
            if (!firebase.IsStudent) return RedirectToAction("Login", "Home");

            return View(new CleaningRequestElseModel());
        }

        [HttpPost]
        public async Task<IActionResult> RequestForElse(CleaningRequestElseModel booking)
        {
            if (!firebase.IsStudent) return RedirectToAction("Login", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    string? targetStudentId = await firebase.FindStudentByExternalId(booking.externalBookingId);

                    if (targetStudentId == null)
                    {
                        ModelState.AddModelError("externalBookingId", "That External Booking ID doesn't match any student.");
                        return View(booking);
                    }

                    await firebase.CreateBooking(targetStudentId, true, booking.roomNumber, booking.bookingDate,
                        booking.roomType, booking.timeSlot, booking.cleaningType, booking.specialInstructions);
                    return RedirectToAction("MyBookings", "Student");
                }
                catch (FirebaseException error)
                {
                    ModelState.AddModelError("", error.Message);
                }
            }

            return View(booking);
        }

        // ------------------------------------------------------------ bookings + notifications

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            if (!firebase.IsStudent) return RedirectToAction("Login", "Home");

            var bookings = await firebase.StudentBookings();
            return View(bookings.Select(BookingMapper.ToView).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> StudentNotifications()
        {
            if (!firebase.IsStudent) return RedirectToAction("Login", "Home");

            var notifications = await firebase.Notifications();
            return View(notifications.Select(BookingMapper.ToView).ToList());
        }

        // ------------------------------------------------------------ profile

        [HttpGet]
        public async Task<IActionResult> StudentProfile()
        {
            if (!firebase.IsStudent) return RedirectToAction("Login", "Home");

            return View(await LoadProfile());
        }

        [HttpPost]
        public async Task<IActionResult> StudentProfile(ProfileModel updated)
        {
            if (!firebase.IsStudent) return RedirectToAction("Login", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    ViewBag.Message = await firebase.UpdateCredentials(updated.currentPassword, updated.email, updated.newPassword);
                }
                catch (FirebaseException error)
                {
                    ViewBag.Message = error.Message;
                }
            }

            ModelState.Clear();   // so the form shows the current values, not what was typed
            return View(await LoadProfile());
        }

        private async Task<ProfileModel> LoadProfile()
        {
            UserProfile? profile = await firebase.GetProfile(firebase.CurrentUid!);
            return new ProfileModel
            {
                name = firebase.CurrentName,
                room = firebase.CurrentRoom,
                email = firebase.CurrentEmail,
                externalBookingId = profile?.ExternalBookingId ?? ""
            };
        }

        // ------------------------------------------------------------ help pages

        public IActionResult StudentHelpSupport()
        {
            return View();
        }

        public IActionResult CleaningGuidelines()
        {
            return View();
        }

        public IActionResult ContactResidenceManagement()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ReportProblem()
        {
            if (!firebase.IsStudent) return RedirectToAction("Login", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ReportProblem(string Issue, string Room, IFormFile Photo)
        {
            if (!firebase.IsStudent) return RedirectToAction("Login", "Home");

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

            try
            {
                // same as the Android app: the report is saved, the photo itself is not uploaded
                await firebase.SubmitProblemReport(Issue, Room, true);
                TempData["SuccessMessage"] = "Report successfully submitted.";
            }
            catch (FirebaseException error)
            {
                TempData["SuccessMessage"] = "Report could not be sent: " + error.Message;
            }

            return RedirectToAction("ReportProblem");
        }

        public IActionResult Logout()
        {
            firebase.Logout();
            return RedirectToAction("Login", "Home");
        }
    }
}
