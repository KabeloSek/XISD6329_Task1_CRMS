using System.ComponentModel.DataAnnotations;

namespace XISD6329_Task1_CRMS.Models
{
    // Cleaner profile page
    public class CleanerProfileModel
    {
        public string? name { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }

        //Firebase needs the current password before it will change the email or password
        public string? currentPassword { get; set; }

        //optional: only filled in if the cleaner wants to change their password
        [MinLength(6, ErrorMessage = "The password must be at least 6 characters")]
        public string? newPassword { get; set; }

        [Compare("newPassword", ErrorMessage = "Passwords do not match")]
        public string? confirmNewPassword { get; set; }
    }
}
