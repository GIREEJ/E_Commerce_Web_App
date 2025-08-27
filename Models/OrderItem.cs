using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.Models
{
    public class OrderItem
    {
        public string OrderItemId { get; set; }

        [Required]
        public string OrderId { get; set; }

        [Required]
        public string ProductId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }

}
