using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.Models
{
    public class CartItem
    {
        public string? CartItemId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string ProductId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public User? User { get; set; }
        public Product? Product { get; set; }
    }

}
