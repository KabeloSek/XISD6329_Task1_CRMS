using System.ComponentModel.DataAnnotations;

namespace XISD6329_Task1_CRMS.Models
{
    // Used by both the student login and the cleaner login
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        public string password { get; set; }
    }
}
