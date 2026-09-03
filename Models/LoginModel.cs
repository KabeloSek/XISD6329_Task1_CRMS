using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace XISD6329_Task1_CRMS.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]// Validates that the email address is in a valid format
        public string email { get; set; }

        //Required field for password
        [Required]
        public string password { get; set; }

        //declaration of global connectionString variable
        private string connection = @"Server=(localdb)\south_point_system;Database=southpoint_database;";

        public bool StudentLogin(string email, string password)
        {
            bool found = false;
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT * FROM Student WHERE Email = '" +email+ "' AND Password = '" +password+ "';";

                    try
                    {
                        using (SqlCommand search = new SqlCommand(query, connect))
                        {
                            using (SqlDataReader reader = search.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    found = true;
                                    Console.WriteLine("User found: " + reader["Name"].ToString());
                                }

                            }//end of using statement for SqlDataReader
                        }//end of using statement for SqlCommand

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error searching for student: " + ex.Message);
                    }
                   
                }//end of using statement for connection
            }
            catch (Exception error)
            {
                Console.WriteLine("User not found: " + error.Message);
            }
            return found;
        }//end of StudentLogin method


    }//end of LoginModel class
}
