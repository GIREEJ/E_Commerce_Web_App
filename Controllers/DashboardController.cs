using ECommerceWebApp.Models;
using Microsoft.AspNetCore.Mvc;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult AdminDashboard()
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        return RedirectToAction("Login", "Account");

        ViewBag.Name = HttpContext.Session.GetString("Name");
        return RedirectToAction("Index");
    }
}
