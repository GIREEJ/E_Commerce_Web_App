using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECommerceWebApp.Data;
using ECommerceWebApp.Models;
using ECommerceWebApp.Services;

namespace ECommerceWebApp.Controllers
{
    public class CartItemsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IdGenerationService _idService;

        public CartItemsController(AppDbContext context, IdGenerationService idService)
        {
            _context = context;
            _idService = idService;
        }

        //GET: CartItems
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.CartItems.Include(c => c.Product).Include(c => c.User);
            return View(await appDbContext.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddCartItem([FromBody] CartItem cartItem)
            {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "User not logged in." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return Unauthorized(new { message = "User not found." });

            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);
            if (product == null)
                return BadRequest(new { message = "Product not found." });

            if (product.Stock < cartItem.Quantity)
                return BadRequest(new { message = "Insufficient stock." });

            // Check if the item already exists in the user's cart
            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == user.UserId && c.ProductId == cartItem.ProductId);

            if (existingCartItem != null)
            {
                // Check if total quantity exceeds stock
                int totalRequested = existingCartItem.Quantity + cartItem.Quantity;
                if (totalRequested > product.Stock)
                    return BadRequest(new { message = "Not enough stock to add more of this item." });

                existingCartItem.Quantity += cartItem.Quantity;
                _context.CartItems.Update(existingCartItem);
            }
            else
            {
                cartItem.CartItemId = _idService.GenerateCartItemId();
                cartItem.UserId = user.UserId;
                _context.CartItems.Add(cartItem);
            }

            product.Stock -= cartItem.Quantity;
            _context.Products.Update(product);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product successfully added to cart!" });
        }


        public async Task<IActionResult> UserCartView()
        {
            var role = HttpContext.Session.GetString("Role");
            var email = HttpContext.Session.GetString("Email");

            if (string.IsNullOrEmpty(role))
            {
                TempData["LoginError"] = "You are not login, Please Login!!!";
                return RedirectToAction("Login", "Account"); 
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Include(c => c.User)
                .Where(c => c.UserId == user.UserId)
                .ToListAsync();
            ViewBag.CartCount = cartItems.Count;
            ViewBag.Email = user.Email;
            return View(cartItems);
        }


        // GET: CartItems/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CartItemId == id);
            if (cartItem == null)
            {
                return NotFound();
            }

            return View(cartItem);
        }

        // GET: CartItems/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName");
            return View();
        }

        // POST: CartItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,ProductId,Quantity")] CartItem cartItem)
        {
            if (ModelState.IsValid)
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
                else
                {
                    product.Stock -= cartItem.Quantity;

                    cartItem.CartItemId = _idService.GenerateCartItemId();
                    _context.Add(cartItem);

                    // Update the product stock
                    _context.Products.Update(product);

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", cartItem.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName", cartItem.UserId);
            return View(cartItem);
        }


        // GET: CartItems/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", cartItem.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName", cartItem.UserId);
            return View(cartItem);
        }

        // POST: CartItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CartItemId,UserId,ProductId,Quantity")] CartItem cartItem)
        {
            if (id != cartItem.CartItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cartItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartItemExists(cartItem.CartItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", cartItem.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName", cartItem.UserId);
            return View(cartItem);
        }

        // GET: CartItems/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CartItemId == id);
            if (cartItem == null)
            {
                return NotFound();
            }

            return View(cartItem);
        }

        // POST: CartItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartItemExists(string id)
        {
            return _context.CartItems.Any(e => e.CartItemId == id);
        }
    }
}
