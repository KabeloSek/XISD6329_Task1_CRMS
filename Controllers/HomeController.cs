using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using XISD6329_Task1_CRMS.Models;

namespace XISD6329_Task1_CRMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        public IActionResult Register()
        {
            //ensure tables exist before the form loads — no ModelState check needed on a fresh GET
            RegisterModel register = new RegisterModel();
            register.Create_tables();
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterModel users)
        {
            if (ModelState.IsValid)
            {
                RegisterModel register = new RegisterModel();
                register.StoreStudent(users.room, users.name, users.email, users.password);

                //only after this succeeds should the student be able to log in
                return RedirectToAction("Login", "Home");
            }
            return View(users);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel user)
        {
            if (ModelState.IsValid)
            {
                LoginModel login = new LoginModel();
                bool found = login.StudentLogin(user.email, user.password, out string studentName, out int studentId, out string studentRoom);

                if (found)
                {
                    HttpContext.Session.SetString("StudentName", studentName);
                    HttpContext.Session.SetString("StudentEmail", user.email);
                    HttpContext.Session.SetInt32("StudentID", studentId);
                    HttpContext.Session.SetString("StudentRoom", studentRoom);

                    return RedirectToAction("StudentHome", "Student");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult RegisterCleaner()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegisterCleaner(CleanerRegisterModel cleaner)
        {
            if (ModelState.IsValid)
            {
                CleanerRegisterModel register = new CleanerRegisterModel();
                register.StoreCleaner(cleaner.name, cleaner.email, cleaner.password);
                return RedirectToAction("LoginCleaner", "Home");
            }
            return View(cleaner);
        }
        [HttpGet]
        public IActionResult LoginCleaner()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoginCleaner(LoginModel cleaner)
        {
            if (ModelState.IsValid)
            {
                LoginModel login = new LoginModel();
                bool found = login.CleanerLogin(cleaner.email, cleaner.password, out string cleanerName, out int cleanerId);

                if (found)
                {
                    HttpContext.Session.SetString("CleanerName", cleanerName);
                    HttpContext.Session.SetString("CleanerEmail", cleaner.email);
                    HttpContext.Session.SetInt32("CleanerID", cleanerId);

                    return RedirectToAction("CleanerHome", "Cleaner");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            return View(cleaner);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
