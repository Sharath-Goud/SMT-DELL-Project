using System.ComponentModel.DataAnnotations;

namespace SMT_DELL_Project.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Employee ID is required.")]
        [StringLength(50, ErrorMessage = "Employee ID cannot exceed 50 characters.")]
        [Display(Name = "Employee ID")]
        public string EmployeeId { get; set; } = string.Empty;


        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(
            "NewPassword",
            ErrorMessage = "Passwords do not match."
        )]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}