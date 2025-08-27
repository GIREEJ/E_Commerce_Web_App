using ECommerceWebApp.Data;
using ECommerceWebApp.Models;
using ECommerceWebApp.Services;
using ECommerceWebApp.Services.Interfaces;
using ECommerceWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ECommerceWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IdGenerationService _idService;

        public AccountController(AppDbContext context, IPasswordService passwordService, IFileUploadService fileUploadService, IdGenerationService idService)
        {
            _context = context;
            _passwordService = passwordService;
            _fileUploadService = fileUploadService;
            _idService = idService;
        }

        // GET: /Account/RegisterAdmin
        public IActionResult RegisterAdmin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAdmin(RegisterAdminVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.AccessId != "NewAdmin@EcommerceWebApp" || model.AccessPassword != "NewAdmin@1234")
            {
                ModelState.AddModelError("", "Invalid access credentials.");
                TempData["RegistrationError"] = "Invalid access credentials.";
                return View(model);
            }

            bool emailExists = await _context.Admins.AnyAsync(a => a.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                TempData["RegistrationError"] = "Email is already registered.";
                return View(model);
            }

            var admin = new Admin
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = _passwordService.HashPassword(model.Password),
                CreatedAt = DateTime.Now
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            TempData["RegistrationSuccess"] = "Admin registered successfully.";
            return RedirectToAction("Login");
        }

        // GET: /Account/RegisterUser
        public async Task<IActionResult> RegisterUser()
        {
            var model = new RegisterUserVM
            {
                Countries = await _context.Countries.ToListAsync(),
                States = new List<State>(),
                Cities = new List<City>()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterUserVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Countries = await _context.Countries.ToListAsync();
                model.States = await _context.States.Where(s => s.CountryId == model.CountryId).ToListAsync();
                model.Cities = await _context.Cities.Where(c => c.StateId == model.StateId).ToListAsync();
                return View(model);
            }

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                TempData["RegistrationError"] = "Email is already registered.";
                model.Countries = await _context.Countries.ToListAsync();
                model.States = await _context.States.Where(s => s.CountryId == model.CountryId).ToListAsync();
                model.Cities = await _context.Cities.Where(c => c.StateId == model.StateId).ToListAsync();
                return View(model);
            }

            string imageUrl = model.ImageFile != null
                ? await _fileUploadService.UploadFileAsync(model.ImageFile)
                : null;

            var user = new User
            {
                UserId = _idService.GenerateUserId(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                Email = model.Email,
                PasswordHash = _passwordService.HashPassword(model.Password),
                ImagePath = imageUrl,
                DateOfBirth = model.DateOfBirth,
                Mobile = model.Mobile,
                Address = model.Address,
                CountryId = model.CountryId,
                StateId = model.StateId,
                CityId = model.CityId,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["RegistrationSuccess"] = "User registered successfully.";
            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var admin = await _context.Admins
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Email == model.Email);

            if (admin != null && _passwordService.VerifyPassword(admin.PasswordHash, model.Password))
            {
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("Name", admin.FullName);
                HttpContext.Session.SetString("Email", admin.Email);
                TempData["LoginSuccess"] = $"Welcome {admin.FullName}, to Admin Dashboard.";
                return RedirectToAction("AdminDashboard", "Dashboard");
            }

            var user = await _context.Users
                 .AsNoTracking()
                 .FirstOrDefaultAsync(a => a.Email == model.Email);
            if (user != null && _passwordService.VerifyPassword(user.PasswordHash, model.Password))
            {
                HttpContext.Session.SetString("Role", "User");
                HttpContext.Session.SetString("Name", user.FirstName ?? "User");
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("UserImage", user.ImagePath ?? "default-user.jfif");
                var cartCount = await _context.CartItems
                                 .AsNoTracking()
                                 .Where(u => u.UserId == user.UserId)
                                 .CountAsync();

                HttpContext.Session.SetInt32("CartCount", cartCount);

                TempData["LoginSuccess"] = "Successfully Login";
                return RedirectToAction("Index", "Home");
                
            }

            ModelState.AddModelError("", "Invalid Email or Password.");
            TempData["LoginError"] = "Invalid Email or Password.";
            return View(model);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Getting states and cities for cascading
        [HttpGet]
        public JsonResult GetStates(int countryId)
        {
            var states = _context.States
                .Where(s => s.CountryId == countryId)
                .Select(s => new { s.StateId, s.StateName })
                .ToList();
            return Json(states);
        }

        [HttpGet]
        public JsonResult GetCities(int stateId)
        {
            var cities = _context.Cities
                .Where(c => c.StateId == stateId)
                .Select(c => new { c.CityId, c.CityName })
                .ToList();
            return Json(cities);
        }

    }
}
