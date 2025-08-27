using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.Models
{
    public class Admin
    {
        public int AdminId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
