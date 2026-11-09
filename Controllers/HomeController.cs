using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using XISD6329_Task1_CRMS.Models;
using XISD6329_Task1_CRMS.Services;

namespace XISD6329_Task1_CRMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly FirebaseService firebase;

        public HomeController(FirebaseService firebase)
        {
            this.firebase = firebase;
        }

        // ------------------------------------------------------------ student register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel users)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await firebase.RegisterStudent(users.room, users.name, users.email, users.password);
                    // only after this succeeds can the student log in
                    return RedirectToAction("Login", "Home");
                }
                catch (FirebaseException error)
                {
                    ModelState.AddModelError("", error.Message);
                }
            }
            return View(users);
        }

        // ------------------------------------------------------------ student login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await firebase.Login(user.email, user.password, "student");
                    return RedirectToAction("StudentHome", "Student");
                }
                catch (FirebaseException error)
                {
                    ModelState.AddModelError("", error.Message);
                }
            }
            return View(user);
        }

        // ------------------------------------------------------------ cleaner register

        [HttpGet]
        public IActionResult RegisterCleaner()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCleaner(CleanerRegisterModel cleaner)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await firebase.RegisterCleaner(cleaner.name, cleaner.email, cleaner.password);
                    return RedirectToAction("LoginCleaner", "Home");
                }
                catch (FirebaseException error)
                {
                    ModelState.AddModelError("", error.Message);
                }
            }
            return View(cleaner);
        }

        // ------------------------------------------------------------ cleaner login

        [HttpGet]
        public IActionResult LoginCleaner()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginCleaner(LoginModel cleaner)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await firebase.Login(cleaner.email, cleaner.password, "cleaner");
                    return RedirectToAction("CleanerHome", "Cleaner");
                }
                catch (FirebaseException error)
                {
                    ModelState.AddModelError("", error.Message);
                }
            }
            return View(cleaner);
        }

        // ------------------------------------------------------------ logout (student and cleaner)

        public IActionResult Logout()
        {
            firebase.Logout();
            return RedirectToAction("Login", "Home");
        }

        public IActionResult LogoutCleaner()
        {
            firebase.Logout();
            return RedirectToAction("LoginCleaner", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
