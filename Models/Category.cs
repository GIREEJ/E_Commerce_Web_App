using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public virtual ICollection<Product>? Products { get; set; }
    }

}
