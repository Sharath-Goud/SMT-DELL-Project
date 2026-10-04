using System.ComponentModel.DataAnnotations;

namespace SMT_DELL_Project.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Employee ID is required.")]
        [StringLength(50)]
        [Display(Name = "Employee ID")]
        public string EmployeeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}