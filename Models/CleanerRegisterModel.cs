using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

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
        public string password { get; set; }

        [Required]
        [Compare("password", ErrorMessage = "Passwords do not match")]
        public string confirmPassword { get; set; }

        private string connection = @"Server=(localdb)\south_point_system;Database=southpoint_database;";

        public void StoreCleaner(string name, string email, string password)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string insertIntoCleaningStaff = @"INSERT INTO CleaningStaff (Name, Email, Password)
                                                         VALUES
                                                            ('" + name + "','" + email + "','" + password + "');";

                    using (SqlCommand insert = new SqlCommand(insertIntoCleaningStaff, connect))
                    {
                        insert.ExecuteNonQuery();
                        Console.WriteLine("Cleaning Staff data inserted successfully");
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Cleaning Staff could not be inserted into database" + error.Message);
            }
        }//end of StoreCleaner
    }
}