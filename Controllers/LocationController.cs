using Microsoft.AspNetCore.Mvc;
using ECommerceWebApp.Data;
using System.Linq;

namespace ECommerceWebApp.Controllers
{
    public class LocationController : Controller
    {
        private readonly AppDbContext _context;
        public LocationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public JsonResult GetStates(int countryId)
        {
            var states = _context.States.Where(s => s.CountryId == countryId).ToList();
            return Json(states);
        }

        [HttpGet]
        public JsonResult GetCities(int stateId)
        {
            var cities = _context.Cities.Where(c => c.StateId == stateId).ToList();
            return Json(cities);
        }
    }
}
