using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.ViewModels
{
    public class ProductVM
    {
        public string? ProductId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public IFormFile? ImageUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } 

        [Required]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }
        public List<SelectListItem>? Categories { get; set; }
    }
}
