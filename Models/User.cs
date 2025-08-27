using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.Models
{
    public class User
    {
        public string UserId { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string? ImagePath { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required, MaxLength(15)]
        public string Mobile { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int StateId { get; set; }

        [Required]
        public int CityId { get; set; }

        [Required]
        public string Role { get; set; } = "User";

        public virtual Country Country { get; set; } // Virtual for lazy loading
        public virtual State State { get; set; } // Virtual for lazy loading
        public virtual City City { get; set; } // Virtual for lazy loading

        public virtual ICollection<Order> Orders { get; set; } // Virtual for lazy loading
        public virtual ICollection<CartItem> CartItems { get; set; } // Virtual for lazy loading
    }
}
