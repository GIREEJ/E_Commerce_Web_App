using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.Models
{
    public class Order
    {
        public string OrderId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required, Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public User User { get; set; }
        public virtual ICollection<OrderItem>? OrderItems { get; set; }
    }

}
