using ECommerceWebApp.Data;
using ECommerceWebApp.Models;
using ECommerceWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWebApp.Controllers
{
    public class ProfilesController : Controller
    {
        public AppDbContext _context;
        public IPasswordService _passwordService;
        public ProfilesController(AppDbContext context, IPasswordService passwordService) 
        {
            _context = context;
            _passwordService = passwordService;
        }
        public IActionResult UserProfile()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users
                .Include(u => u.Country)
                .Include(u => u.State)
                .Include(u => u.City)
                .FirstOrDefault(a => a.Email == email);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Countries = new SelectList(_context.Countries.ToList(), "CountryId", "CountryName");
            ViewBag.States = new SelectList(_context.States.Where(s => s.CountryId == user.CountryId).ToList(), "StateId", "StateName");
            ViewBag.Cities = new SelectList(_context.Cities.Where(c => c.StateId == user.StateId).ToList(), "CityId", "CityName");

            return View(user);
        }

        
        [HttpPost]
        public async Task<IActionResult> UpdateUserProfile(User user, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = new SelectList(_context.Countries.ToList(), "CountryId", "CountryName", user.CountryId);
                ViewBag.States = new SelectList(_context.States.Where(s => s.CountryId == user.CountryId), "StateId", "StateName", user.StateId);
                ViewBag.Cities = new SelectList(_context.Cities.Where(c => c.StateId == user.StateId), "CityId", "CityName", user.CityId);
                return View("UserProfile", user);
            }

            var existingUser = _context.Users.FirstOrDefault(a => a.UserId == user.UserId);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                existingUser.ImagePath = "/images/users/" + fileName;
            }

            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Gender = user.Gender;
            existingUser.DateOfBirth = user.DateOfBirth;
            existingUser.Mobile = user.Mobile;
            existingUser.Address = user.Address;
            existingUser.CountryId = user.CountryId;
            existingUser.StateId = user.StateId;
            existingUser.CityId = user.CityId;

            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("UserProfile");
        }


        public IActionResult AdminProfile()
        {
            // Get AdminId from session
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            var admin = _context.Admins.FirstOrDefault(a => a.Email == email);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        [HttpPost]
        public IActionResult UpdateProfile(Admin admin)
        {
            if (!ModelState.IsValid)
            {
                return View("AdminProfile", admin);
            }

            var existingAdmin = _context.Admins.FirstOrDefault(a => a.AdminId == admin.AdminId);
            if (existingAdmin == null)
            {
                return NotFound();
            }

            existingAdmin.FullName = admin.FullName;
            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("AdminProfile");
        }

        [HttpPost]
        public JsonResult ChangePassword(int id, string newPassword, string oldPassword)
        {

            var admin = _context.Admins.FirstOrDefault(a => a.AdminId == id);
            if (admin == null)
            {
                return Json(new { success = false });
            }
           
            if (admin != null && _passwordService.VerifyPassword(admin.PasswordHash, oldPassword))
            {
                admin.PasswordHash = _passwordService.HashPassword(newPassword);
                _context.Update(admin);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult ChangeUserPassword(string id, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return Json(new { success = false });
            }

            user.PasswordHash = _passwordService.HashPassword(newPassword);
            _context.Users.Update(user);
            _context.SaveChanges();

            return Json(new { success = true });
        }


    }
}
