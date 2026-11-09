using System.ComponentModel.DataAnnotations;

namespace XISD6329_Task1_CRMS.Models
{
    public class CleanerRegisterModel
    {
        [Required]
        public string name { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "The password must be at least 6 characters")]
        public string password { get; set; }

        [Required]
        [Compare("password", ErrorMessage = "Passwords do not match")]
        public string confirmPassword { get; set; }
    }
}