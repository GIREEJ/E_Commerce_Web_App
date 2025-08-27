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
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IdGenerationService _idService;
        private readonly OrderService _orderService;

        public OrdersController(AppDbContext context, IdGenerationService idService, OrderService orderService)
        {
            _context = context;
            _idService = idService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GenerateInvoice(string id)
        {
            try
            {
                byte[] pdfBytes = await _orderService.GenerateOrderPdfAsync(id);

                return File(pdfBytes, "application/pdf", $"Invoice_Order_{id}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to generate invoice: " + ex.Message;
                return RedirectToAction("MyOrders"); // Adjust this based on your action name
            }
        }

        [HttpPost]
        public async Task<IActionResult> Buy([FromBody] EmailRequest request)
        {
            var email = request.Email;
            var productId = request.ProductId;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound(new { message = "User not found." });

            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null) return NotFound(new { message = "Product not found." });

            if (product.Stock <= 0)
                return BadRequest(new { message = "Product is out of stock." });

            // Deduct stock
            product.Stock--;

            var order = new Order
            {
                OrderId = _idService.GenerateOrderId(),
                UserId = user.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = product.Price
            };

            _context.Orders.Add(order);

            var orderItem = new OrderItem
            {
                OrderItemId = _idService.GenerateOrderItemId(),
                OrderId = order.OrderId,
                ProductId = product.ProductId,
                Quantity = 1,
                UnitPrice = product.Price
            };

            _context.OrderItems.Add(orderItem);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Order placed successfully!",
                redirectUrl = Url.Action("MyOrder", "OrderItems", new { id = order.OrderId })
            });
        }


        [HttpPost]
        public async Task<IActionResult> CartBuy([FromBody] EmailRequest request)
        {
            var email = request.Email;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound(new { message = "User not found." });

            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == user.UserId)
                .ToListAsync();

            if (cartItems == null || cartItems.Count == 0)
                return BadRequest(new { message = "Cart is empty." });

            decimal totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);

            var order = new Order
            {
                OrderId = _idService.GenerateOrderId(),
                UserId = user.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderItemId = _idService.GenerateOrderItemId(),
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                };
                _context.OrderItems.Add(orderItem);
                await _context.SaveChangesAsync();
            }

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order placed successfully!" });
        }


        // Action to view order details
        public async Task<IActionResult> ViewDetails(string id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: Orders
        public async Task<IActionResult> Index(string searchString, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderId.Contains(searchString) ||
                    o.User.UserId.Contains(searchString));
            }

            // Sorting
            ViewData["CurrentSort"] = sortOrder;
            ViewData["OrderDateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "orderDate_desc" : "";
            ViewData["TotalAmountSortParm"] = sortOrder == "TotalAmount" ? "totalAmount_desc" : "TotalAmount";

            switch (sortOrder)
            {
                case "orderDate_desc":
                    ordersQuery = ordersQuery.OrderByDescending(o => o.OrderDate);
                    break;
                case "TotalAmount":
                    ordersQuery = ordersQuery.OrderBy(o => o.TotalAmount);
                    break;
                case "totalAmount_desc":
                    ordersQuery = ordersQuery.OrderByDescending(o => o.TotalAmount);
                    break;
                default:
                    ordersQuery = ordersQuery.OrderBy(o => o.OrderDate);
                    break;
            }

            // Pagination
            int totalRecords = await ordersQuery.CountAsync();
            var orders = await ordersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return View(orders);
        }


        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,UserId,OrderDate,TotalAmount")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("OrderId,UserId,OrderDate,TotalAmount")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
