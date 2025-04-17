using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeteoApp.Data;
using MeteoApp.Models;
using MeteoApp.Models.ViewModels;
using MeteoApp.Services;

namespace MeteoApp.Controllers
{
    public class WeatherDatasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AccuWeatherService _accuService;

        public WeatherDatasController(AppDbContext context, AccuWeatherService accuService)
        {
            _context = context;
            _accuService = accuService;
        }

        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        }

        public async Task<IActionResult> Index(string? filtroUtente)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            var username = HttpContext.Session.GetString("Username");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            ViewBag.Success = TempData["Success"];

            if (isAdmin)
            {
                var utenti = await _context.WeatherData
                    .Where(w => !string.IsNullOrEmpty(w.Username))
                    .Select(w => w.Username)
                    .Distinct()
                    .ToListAsync();

                ViewBag.Utenti = utenti;
                ViewBag.FiltroUtente = filtroUtente;

                var dati = string.IsNullOrEmpty(filtroUtente)
                    ? await _context.WeatherData.ToListAsync()
                    : await _context.WeatherData.Where(w => w.Username == filtroUtente).ToListAsync();

                return View(dati);
            }
            else
            {
                var dati = await _context.WeatherData.Where(w => w.Username == username).ToListAsync();
                return View(dati);
            }
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (id == null) return NotFound();

            var weatherData = await _context.WeatherData.FirstOrDefaultAsync(m => m.Id == id);
            if (weatherData == null) return NotFound();

            return View(weatherData);
        }

        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LocationName,Timestamp,CurrentTemperature")] WeatherData weatherData)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                _context.Add(weatherData);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Dati inseriti manualmente con successo.";
                return RedirectToAction(nameof(Index));
            }
            return View(weatherData);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (id == null) return NotFound();

            var data = await _context.WeatherData.FindAsync(id);
            if (data == null) return NotFound();

            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            if (isAdmin)
            {
                var utenti = await _context.Users.Select(u => u.Username).Distinct().ToListAsync();
                ViewBag.Utenti = utenti;
            }

            return View(data);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LocationName,Timestamp,CurrentTemperature,Username")] WeatherData data)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (id != data.Id) return NotFound();

            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            if (!isAdmin)
            {
                
                data.Username = HttpContext.Session.GetString("Username");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(data);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Previsione aggiornata con successo.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.WeatherData.Any(e => e.Id == data.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            if (isAdmin)
            {
                ViewBag.Utenti = await _context.Users.Select(u => u.Username).Distinct().ToListAsync();
            }

            return View(data);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (id == null) return NotFound();

            var weatherData = await _context.WeatherData.FirstOrDefaultAsync(m => m.Id == id);
            if (weatherData == null) return NotFound();

            return View(weatherData);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            var weatherData = await _context.WeatherData.FindAsync(id);
            if (weatherData != null)
            {
                _context.WeatherData.Remove(weatherData);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Dati eliminati correttamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool WeatherDataExists(int id)
        {
            return _context.WeatherData.Any(e => e.Id == id);
        }

        [HttpGet]
        public IActionResult Previsioni()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Previsioni(string city)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            var locationKey = await _accuService.GetLocationKeyAsync(city);
            if (locationKey == null)
            {
                ViewBag.Error = "Località non trovata.";
                return View();
            }

            var currentJson = await _accuService.GetCurrentConditionsAsync(locationKey);
            var forecastJson = await _accuService.GetForecastAsync(locationKey);

            using var doc = JsonDocument.Parse(currentJson);
            var temperature = doc.RootElement[0].GetProperty("Temperature").GetProperty("Metric").GetProperty("Value").GetSingle();
            var timestamp = doc.RootElement[0].GetProperty("LocalObservationDateTime").GetDateTime();

            var model = new WeatherResultViewModel
            {
                City = city,
                LocationKey = locationKey,
                CurrentConditionsJson = currentJson,
                ForecastJson = forecastJson,
                TemperatureCelsius = temperature,
                Timestamp = timestamp
            };

            return View("Previsioni", model);
        }
        [HttpPost]
        public async Task<IActionResult> SalvaPrevisioni(WeatherResultViewModel model, string mode)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            if (mode == "oggi")
            {
                var data = new WeatherData
                {
                    LocationName = model.City,
                    Timestamp = model.Timestamp,
                    CurrentTemperature = model.TemperatureCelsius ?? 0,
                    Username = HttpContext.Session.GetString("Username") 
                };


                _context.WeatherData.Add(data);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Dati meteo odierni per {model.City} salvati con successo!";
                return RedirectToAction("Index");
            }
            else if (mode == "tutti")
            {
                try
                {
                    var username = HttpContext.Session.GetString("Username"); 
                    var doc = JsonDocument.Parse(model.ForecastJson);
                    var giorni = doc.RootElement.GetProperty("DailyForecasts");

                    foreach (var giorno in giorni.EnumerateArray())
                    {
                        var date = giorno.GetProperty("Date").GetDateTime();
                        var temperature = giorno.GetProperty("Temperature").GetProperty("Maximum").GetProperty("Value").GetDouble();

                        var data = new WeatherData
                        {
                            LocationName = model.City,
                            Timestamp = date,
                            CurrentTemperature = (float)temperature,
                            Username = username 
                        };

                        _context.WeatherData.Add(data);
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Previsioni per i prossimi 5 giorni a {model.City} salvate con successo!";
                }
                catch
                {
                    TempData["Error"] = "Errore durante il salvataggio delle previsioni.";
                }

                return RedirectToAction("Index");
            }


            TempData["Error"] = "Nessuna modalità di salvataggio selezionata.";
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Grafico()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            var dati = await _context.WeatherData
                .OrderByDescending(w => w.Timestamp)
                .Take(7)
                .ToListAsync();

            return View(dati.OrderBy(w => w.Timestamp).ToList()); 
        }

    }
}
