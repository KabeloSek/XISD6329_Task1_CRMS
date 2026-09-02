using System.ComponentModel.DataAnnotations;

namespace XISD6329_Task1_CRMS.Models
{
    public class RegisterModel 
    {
        //Required field for room number selected from dropdown
        [Required]
        public string room { get; set; }

        //Required field for student name
        [Required]
        public string name { get; set; }

        //Required field for student email
        [Required]
        [EmailAddress]// Validates that the email address is in a valid format
        public string email { get; set; }

        //Required field for password
        [Required]
        public string password { get; set; }

        //Required field for password
        [Required]
        [Compare("password", ErrorMessage = "Passwords do not match")]// Validates that the confirm password matches the password)]
        public string confirmPassword { get; set; }

        //declaration of global connectionString variable
        private string connection = @"Server=(localdb)\south_point_system;Database=southpoint_database;";

        public string Create_Student_table()
        {
            //try and catch for error handling
            try
            {

            }
            catch (Exception error)
            {

            }
            return "Student table created successfully";
        }//end of Create_Student_table method

    }

}
