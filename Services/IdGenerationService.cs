using ECommerceWebApp.Data;

namespace ECommerceWebApp.Services
{
    public class IdGenerationService
    {
        private readonly AppDbContext _context;

        public IdGenerationService(AppDbContext context)
        {
            _context = context;
        }

        // Generate ID for User (e.g., Cus001, Cus002)
        public string GenerateUserId()
        {
            var lastUser = _context.Users
                .OrderByDescending(p => p.UserId)
                .FirstOrDefault();

            int nextNumber = lastUser == null
                ? 1
                : int.Parse(lastUser.UserId.Substring(4)) + 1;

            return $"Cust{nextNumber.ToString("D4")}";
        }

        // Generate ID for Product (e.g., Prod001, Prod002)
        public string GenerateProductId()
        {
            var lastProduct = _context.Products
                .OrderByDescending(p => p.ProductId)
                .FirstOrDefault();

            int nextNumber = lastProduct == null
                ? 1
                : int.Parse(lastProduct.ProductId.Substring(4)) + 1;

            return $"Prod{nextNumber.ToString("D3")}";
        }

        // Generate ID for Order (e.g., Ord001, Ord002)
        public string GenerateOrderId()
        {
            var lastOrder = _context.Orders
                .OrderByDescending(o => o.OrderId)
                .FirstOrDefault();

            int nextNumber = lastOrder == null
                ? 1
                : int.Parse(lastOrder.OrderId.Substring(3)) + 1;

            return $"Ord{nextNumber.ToString("D4")}";
        }

        // Generate ID for OrderItem (e.g., OI001, OI002)
        public string GenerateOrderItemId()
        {
            var lastOrderItem = _context.OrderItems
                .OrderByDescending(oi => oi.OrderItemId)
                .FirstOrDefault();

            int nextNumber = lastOrderItem == null
                ? 1
                : int.Parse(lastOrderItem.OrderItemId.Substring(2)) + 1;

            return $"OI{nextNumber.ToString("D3")}";
        }

        // Generate ID for CartItem (e.g., CI001, CI002)
        public string GenerateCartItemId()
        {
            var lastCartItem = _context.CartItems
                .OrderByDescending(ci => ci.CartItemId)
                .FirstOrDefault();

            int nextNumber = lastCartItem == null
                ? 1
                : int.Parse(lastCartItem.CartItemId.Substring(2)) + 1;

            return $"CI{nextNumber.ToString("D3")}";
        }
    }

}
