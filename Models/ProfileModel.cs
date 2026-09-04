using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace XISD6329_Task1_CRMS.Models
{
    public class ProfileModel
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }

        //fetched so it can be shown/hidden, not for editing directly
        public string password { get; set; }

        //optional — only filled in if the student wants to change their password
        public string newPassword { get; set; }

        [Compare("newPassword", ErrorMessage = "Passwords do not match")]
        public string confirmNewPassword { get; set; }

        public string externalBookingId { get; set; }

        private string connection = @"Server=(localdb)\south_point_system;Database=southpoint_database;Trusted_Connection=True;";

        public ProfileModel Get_Student(string currentEmail)
        {
            ProfileModel student = null;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT Email, Password, ExternalBookingID FROM Student WHERE Email=@Email";

                    using (SqlCommand command = new SqlCommand(query, connect))
                    {
                        command.Parameters.AddWithValue("@Email", currentEmail);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                student = new ProfileModel
                                {
                                    email = reader["Email"].ToString(),
                                    password = reader["Password"].ToString(),
                                    externalBookingId = reader["ExternalBookingID"].ToString()
                                };
                            }
                        }
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error fetching student profile: " + error.Message);
            }

            return student;
        }//end Get_Student

        public bool Update_Student(string currentEmail, string newEmail, string newPassword)
        {
            bool success = false;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query;
                    if (!string.IsNullOrWhiteSpace(newPassword))
                    {
                        query = @"UPDATE Student SET Email=@NewEmail, Password=@NewPassword WHERE Email=@CurrentEmail";
                    }
                    else
                    {
                        query = @"UPDATE Student SET Email=@NewEmail WHERE Email=@CurrentEmail";
                    }

                    using (SqlCommand command = new SqlCommand(query, connect))
                    {
                        command.Parameters.AddWithValue("@NewEmail", newEmail);
                        command.Parameters.AddWithValue("@CurrentEmail", currentEmail);

                        if (!string.IsNullOrWhiteSpace(newPassword))
                        {
                            command.Parameters.AddWithValue("@NewPassword", newPassword);
                        }

                        int rowsAffected = command.ExecuteNonQuery();
                        success = rowsAffected > 0;
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error updating student profile: " + error.Message);
            }

            return success;
        }//end Update_Student
    }
}