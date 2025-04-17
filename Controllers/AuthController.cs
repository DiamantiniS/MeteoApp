using Microsoft.AspNetCore.Mvc;
using MeteoApp.Data;
using MeteoApp.Models;
using MeteoApp.Models.ViewModels;

namespace MeteoApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        
        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Inserisci username e password.";
                return View();
            }

            if (username == _config["AdminCredentials:Username"] && password == _config["AdminCredentials:Password"])
            {
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Index", "WeatherDatas");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username ?? "");
                HttpContext.Session.SetString("IsAdmin", "false");
                return RedirectToAction("Index", "WeatherDatas");
            }

            ViewBag.Error = "Login fallito.";
            return View();
        }

        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ViewBag.Error = "Username già esistente.";
                return View(model);
            }

            var user = new User
            {
                Nome = model.Nome,
                Cognome = model.Cognome,
                Username = model.Username,
                Password = model.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Username", user.Username ?? "");
            HttpContext.Session.SetString("IsAdmin", "false");

            return RedirectToAction("Index", "WeatherDatas");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
