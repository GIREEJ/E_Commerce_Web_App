using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECommerceWebApp.Data;
using ECommerceWebApp.Models;
using ECommerceWebApp.ViewModels;
using ECommerceWebApp.Services;
using ECommerceWebApp.Services.Interfaces;

namespace ECommerceWebApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly IPasswordService _passwordService;
        private readonly IdGenerationService _idGenerationService;
        public UsersController(AppDbContext context, IFileUploadService fileUploadService, IPasswordService passwordService, IdGenerationService idGenerationService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _passwordService = passwordService;
            _idGenerationService = idGenerationService;
        }

        // GET: Users
        public async Task<IActionResult> Index(string name)
        {
            
            var usersQuery = _context.Users
                .Include(u => u.City)
                .Include(u => u.State)
                .Include(u => u.Country)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                usersQuery = usersQuery.Where(u =>
                    u.FirstName.Contains(name.ToLower().Trim()) || 
                    u.LastName.Contains(name.ToLower().Trim()) || 
                    u.Email.Contains(name.ToLower().Trim()) ||
                    u.Mobile.Contains(name.ToLower().Trim()) || 
                    u.Address.Contains(name.ToLower().Trim()) ||
                    u.Gender.Contains(name.ToLower().Trim()
                    
                    ));
            }

            return View(await usersQuery.ToListAsync());
        }


        // GET: Users/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.City)
                .Include(u => u.Country)
                .Include(u => u.State)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            var model = new RegisterUserVM
            {
                Countries = await _context.Countries.ToListAsync(),
                States = new List<State>(),
                Cities = new List<City>()
            };

            ViewBag.CountryId = new SelectList(model.Countries, "CountryId", "CountryName");
            ViewBag.StateId = new SelectList(model.States, "StateId", "StateName");
            ViewBag.CityId = new SelectList(model.Cities, "CityId", "CityName");

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Create(RegisterUserVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Countries = await _context.Countries.ToListAsync();
                model.States = await _context.States.Where(s => s.CountryId == model.CountryId).ToListAsync();
                model.Cities = await _context.Cities.Where(c => c.StateId == model.StateId).ToListAsync();

                ViewBag.CountryId = new SelectList(model.Countries, "CountryId", "CountryName");
                ViewBag.StateId = new SelectList(model.States, "StateId", "StateName");
                ViewBag.CityId = new SelectList(model.Cities, "CityId", "CityName");

                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                TempData["RegistrationError"] = "Email is already registered.";
                return await Create(); // reload with dropdowns
            }

            var imagePath = model.ImageFile != null
                ? await _fileUploadService.UploadFileAsync(model.ImageFile)
                : null;

            var defaultPassword = "User@1234";

            var user = new User
            {
                UserId = _idGenerationService.GenerateUserId(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                Email = model.Email,
                PasswordHash = _passwordService.HashPassword(defaultPassword),
                ImagePath = imagePath,
                DateOfBirth = model.DateOfBirth,
                Mobile = model.Mobile,
                Address = model.Address,
                CountryId = model.CountryId,
                StateId = model.StateId,
                CityId = model.CityId,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["RegistrationSuccess"] = "User registered successfully with default password.";
            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["DeleteSuccess"] = "User Successfully Deleted";
            }

            return RedirectToAction(nameof(Index));
        }
        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var model = new RegisterUserVM
            {
                UserId = id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Mobile = user.Mobile,
                Address = user.Address,
                CountryId = user.CountryId,
                StateId = user.StateId,
                CityId = user.CityId,
                ExistingImagePath = user.ImagePath,
            };

            LoadDropDowns(user.CountryId, user.StateId, user.CityId);
            ViewData["ExistingImagePath"] = user.ImagePath;
            return View(model);
        }


        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string? id, RegisterUserVM model)
        {
            if (id != model.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound();

                // Handle Image Upload
                if (model.ImageFile != null)
                {
                    string imageUrl = await _fileUploadService.UploadFileAsync(model.ImageFile);
                    user.ImagePath = imageUrl; // <-- Assign it to user.ImagePath
                }
                else if (!string.IsNullOrEmpty(model.ExistingImagePath))
                {
                    user.ImagePath = model.ExistingImagePath;
                }
                else if (!string.IsNullOrEmpty(model.ExistingImagePath))
                {
                    user.ImagePath = model.ExistingImagePath;
                }

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    user.PasswordHash = _passwordService.HashPassword(model.Password);
                }

                // Other fields
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.Mobile = model.Mobile;
                user.Gender = model.Gender;
                user.DateOfBirth = model.DateOfBirth;
                user.Address = model.Address;
                user.CountryId = model.CountryId;
                user.StateId = model.StateId;
                user.CityId = model.CityId;
               
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["EditSuccess"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            LoadDropDowns(model.CountryId, model.StateId, model.CityId);
            return View(model);
        }

        public void ResetPassword(string userId)
        {
           var user= _context.Users.FirstOrDefault(u => u.UserId == userId);
            user.PasswordHash = _passwordService.HashPassword("User@1234");
            _context.Update(user);
            _context.SaveChanges();

        }

        private void LoadDropDowns(int? countryId = null, int? stateId = null, int? cityId = null)
        {
            // Always load countries
            var countries = _context.Countries?.ToList() ?? new List<Country>();
            ViewData["CountryId"] = new SelectList(countries, "CountryId", "CountryName", countryId);

            // Load states only if countryId is provided
            var states = new List<State>();
            if (countryId.HasValue)
            {
                states = _context.States?.Where(s => s.CountryId == countryId.Value).ToList() ?? new List<State>();
            }
            ViewData["StateId"] = new SelectList(states, "StateId", "StateName", stateId);

            // Load cities only if stateId is provided
            var cities = new List<City>();
            if (stateId.HasValue)
            {
                cities = _context.Cities?.Where(c => c.StateId == stateId.Value).ToList() ?? new List<City>();
            }
            ViewData["CityId"] = new SelectList(cities, "CityId", "CityName", cityId);
        }
    }
}
