using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.Models
{
    public class Country
    {
        public int CountryId { get; set; }

        [Required, MaxLength(50)]
        public string CountryName { get; set; }

        public virtual ICollection<State>? States { get; set; }
        public virtual ICollection<User>? Users { get; set; } // Virtual for lazy loading
    }
}
