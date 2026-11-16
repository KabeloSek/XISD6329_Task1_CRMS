using System.ComponentModel.DataAnnotations;

namespace XISD6329_Task1_CRMS.Models
{
    public class CleanerRegisterModel
    {
        [Required]
        public string name { get; set; }

        [Required]
        [RegularExpression(Validation.EmailPattern, ErrorMessage = Validation.EmailMessage)]
        public string email { get; set; }

        [Required]
        [RegularExpression(Validation.PasswordPattern, ErrorMessage = Validation.PasswordMessage)]
        public string password { get; set; }

        [Required]
        [Compare("password", ErrorMessage = "Passwords do not match")]
        public string confirmPassword { get; set; }
    }
}