using Microsoft.AspNetCore.Mvc;
using XISD6329_Task1_CRMS.Models;
using XISD6329_Task1_CRMS.Services;

namespace XISD6329_Task1_CRMS.Controllers
{
    public class CleanerController : Controller
    {
        private readonly FirebaseService firebase;

        public CleanerController(FirebaseService firebase)
        {
            this.firebase = firebase;
        }

        public IActionResult CleanerHome()
        {
            if (!firebase.IsCleaner)
                return RedirectToAction("LoginCleaner", "Home");

            return View();
        }

        // every cleaner sees every request that is still Pending
        public async Task<IActionResult> CleanerRequests()
        {
            if (!firebase.IsCleaner)
                return RedirectToAction("LoginCleaner", "Home");

            var bookings = await firebase.OpenRequests();
            return View(bookings.Select(BookingMapper.ToView).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> AcceptBooking(string bookingId)
        {
            if (!firebase.IsCleaner)
                return RedirectToAction("LoginCleaner", "Home");

            bool accepted = await firebase.AcceptBooking(bookingId);
            if (!accepted)
            {
                TempData["Error"] = "That request was already taken by another cleaner.";
            }

            return RedirectToAction("CleanerRequests");
        }

        public async Task<IActionResult> CleanerBookings()
        {
            if (!firebase.IsCleaner)
                return RedirectToAction("LoginCleaner", "Home");

            var bookings = await firebase.CleanerBookings();
            return View(bookings.Select(BookingMapper.ToView).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> CompleteBooking(string bookingId, string passkey)
        {
            if (!firebase.IsCleaner)
                return RedirectToAction("LoginCleaner", "Home");

            bool success = await firebase.CompleteBooking(bookingId, passkey ?? "");
            if (!success)
            {
                TempData["Error"] = "Incorrect passkey — ask the student to confirm it.";
            }

            return RedirectToAction("CleanerBookings");
        }

        [HttpGet]
        public IActionResult CleanerProfile()
        {
            if (!firebase.IsCleaner)
                return RedirectToAction("LoginCleaner", "Home");

            return View(new CleanerProfileModel { name = firebase.CurrentName, email = firebase.CurrentEmail });
        }

        [HttpPost]
        public async Task<IActionResult> CleanerProfile(CleanerProfileModel updated)
        {
            if (!firebase.IsCleaner)
                return RedirectToAction("LoginCleaner", "Home");

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

            // always show the current email (a changed email only applies after the verification link)
            return View(new CleanerProfileModel { name = firebase.CurrentName, email = firebase.CurrentEmail });
        }
    }
}
