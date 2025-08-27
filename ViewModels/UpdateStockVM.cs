using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.ViewModels
{
    public class UpdateStockVM
    {
        public string? ProductId { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}
