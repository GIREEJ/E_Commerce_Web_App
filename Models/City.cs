using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.Models
{
    public class City
    {
        public int CityId { get; set; }

        [Required, MaxLength(50)]
        public string CityName { get; set; }

        [Required]
        public int StateId { get; set; }

        public virtual State State { get; set; } // Virtual for lazy loading
        public virtual ICollection<User> Users { get; set; } // Virtual for lazy loading
    }
}
