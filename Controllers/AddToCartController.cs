using ECommerceWebApp.Data;
using ECommerceWebApp.Models;
using ECommerceWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWebApp.Controllers
{
    public class AddToCartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IdGenerationService _idService;
        private readonly CouponService _couponService;

        public AddToCartController(AppDbContext context, IdGenerationService idService, CouponService couponService)
        {
            _context = context;
            _idService = idService;
            _couponService = couponService;
        }

        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string couponCode, bool buyNow, [Bind("UserId,ProductId,Quantity")] CartItem cartItem)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);

            if (product == null)
            {
                ModelState.AddModelError("ProductId", "Selected product does not exist.");
            }
            else if (product.Stock < cartItem.Quantity)
            {
                ModelState.AddModelError("Quantity", "Insufficient stock available.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", cartItem.ProductId);
                ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName", cartItem.UserId);
                return View(cartItem);
            }

            decimal discount = 0;
            if (!string.IsNullOrEmpty(couponCode))
            {
                if (_couponService.IsValidCoupon(couponCode))
                {
                    discount = _couponService.GetDiscountPercentage(couponCode);
                    ViewBag.DiscountMessage = $"Coupon applied! {discount}% discount.";
                }
                else
                {
                    ModelState.AddModelError("Coupon", "Invalid or expired coupon code.");
                }
            }

            // Apply discount logic (e.g., logging or updating cart totals - depends on your order flow)
            cartItem.CartItemId = _idService.GenerateCartItemId();
            _context.CartItems.Add(cartItem);

            // Reduce stock
            product.Stock -= cartItem.Quantity;
            _context.Products.Update(product);

            await _context.SaveChangesAsync();

            if (buyNow)
                return RedirectToAction("Create", "Orders", new { userId = cartItem.UserId }); // Redirect to Buy Now

            return RedirectToAction("Index", "CartItems");
        }
    }
}
