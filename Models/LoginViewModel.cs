using System.ComponentModel.DataAnnotations;

namespace ST10296771_CLDV7311_POE.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username or Email is required")]
        [Display(Name = "Username or Email")]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "Login as")]
        public string Role { get; set; }
    }
}
