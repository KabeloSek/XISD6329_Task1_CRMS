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

        public bool StudentLogin(string email, string password, out string studentName, out int studentId, out string studentRoom)
        {
            bool found = false;
            studentName = "";
            studentId = 0;
            studentRoom = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT * FROM Student WHERE Email=@Email AND Password=@Password";

                    using (SqlCommand command = new SqlCommand(query, connect))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                found = true;
                                studentName = reader["Name"].ToString();
                                studentId = (int)reader["StudentID"];
                                studentRoom = reader["Room"].ToString();
                            }
                        }
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error during student login: " + error.Message);
            }

            return found;
        }

        public bool CleanerLogin(string email, string password, out string cleanerName)
        {
            bool found = false;
            cleanerName = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();
                    string query = @"SELECT * FROM Cleaner WHERE Email=@Email AND Password=@Password";
                    using (SqlCommand command = new SqlCommand(query, connect))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                found = true;
                                cleanerName = reader["Name"].ToString();
                            }
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error during cleaner login: " + error.Message);
            }
            return found;
        }//end of CleanerLogin method
    }//end of LoginModel class
}
