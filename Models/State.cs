using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.Models
{
    public class State
    {
        public int StateId { get; set; }

        [Required, MaxLength(50)]
        public string StateName { get; set; }

        [Required]
        public int CountryId { get; set; }

        public virtual Country Country { get; set; } // Virtual for lazy loading
        public virtual ICollection<City> Cities { get; set; } // Virtual for lazy loading
        public virtual ICollection<User> Users { get; set; } // Virtual for lazy loading
    }
}
