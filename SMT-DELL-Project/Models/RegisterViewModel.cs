using System.ComponentModel.DataAnnotations;

namespace SMT_DELL_Project.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Employee ID is required.")]
        [StringLength(50, ErrorMessage = "Employee ID cannot exceed 50 characters.")]
        [Display(Name = "Employee ID")]
        public string EmployeeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}