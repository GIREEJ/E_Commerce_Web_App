using System.ComponentModel.DataAnnotations;

namespace ECommerceWebApp.ViewModels
{
    public class RegisterAdminVM
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name ="Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Email Id")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required."), MinLength(6), DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required."), Compare("Password"), DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Access Id is required.")]
        [Display(Name = "Access Id")]
        public string AccessId { get; set; }

        [Required(ErrorMessage = "Access Password is required.")]
        [Display(Name = "Access Password")]
        public string AccessPassword { get; set; } 
    }

}
