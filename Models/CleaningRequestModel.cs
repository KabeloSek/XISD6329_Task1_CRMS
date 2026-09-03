using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace XISD6329_Task1_CRMS.Models
{
    public class CleaningRequestModel
    {
        public int studentId { get; set; }

        [Required]
        public string roomNumber { get; set; }

        [Required]
        public DateTime bookingDate { get; set; }

        [Required]
        public string roomType { get; set; }

        [Required]
        public string timeSlot { get; set; }

        [Required]
        public string cleaningType { get; set; }

        [Required(ErrorMessage = "Enter the External Booking ID of the student you're booking for")]
        public string externalBookingId { get; set; }

        public string specialInstructions { get; set; }

        private string connection = @"Server=(localdb)\south_point_system;Database=southpoint_database;Trusted_Connection=True;";

        public void StoreBooking(int studentId, string roomNumber, DateTime bookingDate, string roomType, string timeSlot, string cleaningType, string specialInstructions)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    //generate a unique passkey the student will give the cleaner to confirm completion
                    string passkey = GenerateUniquePasskey(connect);

                    string insertIntoBooking = @"INSERT INTO Booking (StudentID, RoomNumber, BookingDate, RoomType, TimeSlot, CleaningType, SpecialInstructions, Status, Passkey)
                                                  OUTPUT INSERTED.BookingID
                                                  VALUES
                                                  (@StudentID, @RoomNumber, @BookingDate, @RoomType, @TimeSlot, @CleaningType, @SpecialInstructions, 'Pending', @Passkey)";

                    int bookingId;
                    using (SqlCommand insert = new SqlCommand(insertIntoBooking, connect))
                    {
                        insert.Parameters.AddWithValue("@StudentID", studentId);
                        insert.Parameters.AddWithValue("@RoomNumber", roomNumber);
                        insert.Parameters.AddWithValue("@BookingDate", bookingDate);
                        insert.Parameters.AddWithValue("@RoomType", roomType);
                        insert.Parameters.AddWithValue("@TimeSlot", timeSlot);
                        insert.Parameters.AddWithValue("@CleaningType", cleaningType);
                        insert.Parameters.AddWithValue("@SpecialInstructions", (object)specialInstructions ?? DBNull.Value);
                        insert.Parameters.AddWithValue("@Passkey", passkey);

                        bookingId = (int)insert.ExecuteScalar();
                    }

                    //notify the student immediately with their passkey, so they have it ready for the cleaner
                    string insertNotification = @"INSERT INTO Notification (StudentID, Message, NotificationType, BookingID, Passkey, BookingDate, CreatedAt)
                                                   VALUES
                                                   (@StudentID, @Message, 'Requested', @BookingID, @Passkey, @BookingDate, GETDATE())";

                    using (SqlCommand notify = new SqlCommand(insertNotification, connect))
                    {
                        notify.Parameters.AddWithValue("@StudentID", studentId);
                        notify.Parameters.AddWithValue("@Message", "Your cleaning request has been submitted. Give this passkey to your cleaner once the job is done.");
                        notify.Parameters.AddWithValue("@BookingID", bookingId);
                        notify.Parameters.AddWithValue("@Passkey", passkey);
                        notify.Parameters.AddWithValue("@BookingDate", bookingDate);
                        notify.ExecuteNonQuery();
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Booking could not be inserted: " + error.Message);
            }
        }//end StoreBooking

        //keeps generating until it finds one not already used — passkeys are short, so collisions are possible
        private string GenerateUniquePasskey(SqlConnection connect)
        {
            string passkey;
            bool exists;

            do
            {
                passkey = new Random().Next(100000, 999999).ToString();

                string checkQuery = @"SELECT COUNT(*) FROM Booking WHERE Passkey=@Passkey";
                using (SqlCommand check = new SqlCommand(checkQuery, connect))
                {
                    check.Parameters.AddWithValue("@Passkey", passkey);
                    exists = (int)check.ExecuteScalar() > 0;
                }
            } while (exists);

            return passkey;
        }//end GenerateUniquePasskey


        //finds the StudentID belonging to the given ExternalBookingID, so the booking is
        //recorded against the actual student it's for, not whoever is logged in
        public int? FindStudentByExternalId(string externalBookingId)
        {
            int? studentId = null;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT StudentID FROM Student WHERE ExternalBookingID=@ExternalBookingID";

                    using (SqlCommand command = new SqlCommand(query, connect))
                    {
                        command.Parameters.AddWithValue("@ExternalBookingID", externalBookingId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                studentId = (int)reader["StudentID"];
                            }
                        }
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error validating External Booking ID: " + error.Message);
            }

            return studentId;
        }//end FindStudentByExternalId

        
    }
}
