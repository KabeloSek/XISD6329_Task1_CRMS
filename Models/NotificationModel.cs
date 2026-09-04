using System.Data.SqlClient;

namespace XISD6329_Task1_CRMS.Models
{
    public class NotificationViewModel
    {
        public string message { get; set; }
        public string notificationType { get; set; }
        public string cleanerName { get; set; }
        public string passkey { get; set; }
        public DateTime? bookingDate { get; set; }
        public DateTime createdAt { get; set; }
    }

    public class NotificationModel
    {
        private string connection = @"Server=(localdb)\south_point_system;Database=southpoint_database;Trusted_Connection=True;";

        public List<NotificationViewModel> GetNotifications(int studentId)
        {
            List<NotificationViewModel> notifications = new List<NotificationViewModel>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT * FROM Notification WHERE StudentID=@StudentID ORDER BY CreatedAt DESC";

                    using (SqlCommand command = new SqlCommand(query, connect))
                    {
                        command.Parameters.AddWithValue("@StudentID", studentId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                notifications.Add(new NotificationViewModel
                                {
                                    message = reader["Message"].ToString(),
                                    notificationType = reader["NotificationType"] == DBNull.Value ? "" : reader["NotificationType"].ToString(),
                                    cleanerName = reader["CleanerName"] == DBNull.Value ? null : reader["CleanerName"].ToString(),
                                    passkey = reader["Passkey"] == DBNull.Value ? null : reader["Passkey"].ToString(),
                                    bookingDate = reader["BookingDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["BookingDate"],
                                    createdAt = (DateTime)reader["CreatedAt"]
                                });
                            }
                        }
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error fetching notifications: " + error.Message);
            }

            return notifications;
        }//end GetNotifications
    }
}