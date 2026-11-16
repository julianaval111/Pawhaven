
using System.ComponentModel.DataAnnotations;

namespace Pawhaven.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Username or Email")]
        [Required(ErrorMessage = "Please enter your username or email.")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
