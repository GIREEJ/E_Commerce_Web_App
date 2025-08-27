using ECommerceWebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.ViewModels
{
    public class EditUserVM
    {
        public string? UserId { get; set; }
        [Required(ErrorMessage = "First Name is required."), MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required."), MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Email is required."), EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required."), DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required."), Compare("Password"), DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Date of Birth is required."), DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Mobile is required."), MaxLength(15)]
        [Display(Name = "Mobile no.")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        [Display(Name = "Select Country")]
        public int CountryId { get; set; }

        [Required(ErrorMessage = "State is required.")]
        [Display(Name = "Select State")]
        public int StateId { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [Display(Name = "Select City")]
        public int CityId { get; set; }
        public string? ExistingImagePath { get; set; }
        public List<Country>? Countries { get; set; }
        public List<State>? States { get; set; }
        public List<City>? Cities { get; set; }
    }
}
