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
                bool found = login.StudentLogin(user.email, user.password);

                if (found)
                {
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
        public IActionResult RegisterCleaner(RegisterModel users)
        {
            if (!ModelState.IsValid)
            {
                RegisterModel register = new RegisterModel();
                register.StoreCleaner(users.name, users.email, users.password);
            }
            return View(users);
        }

        public IActionResult LoginCleaner()
        {
            return View();
        }
       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
