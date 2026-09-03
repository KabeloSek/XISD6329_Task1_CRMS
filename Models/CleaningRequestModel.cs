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

                    string insertIntoBooking = @"INSERT INTO Booking (StudentID, RoomNumber, BookingDate, RoomType, TimeSlot, CleaningType, SpecialInstructions)
                                                  VALUES
                                                  (@StudentID, @RoomNumber, @BookingDate, @RoomType, @TimeSlot, @CleaningType, @SpecialInstructions)";

                    using (SqlCommand insert = new SqlCommand(insertIntoBooking, connect))
                    {
                        insert.Parameters.AddWithValue("@StudentID", studentId);
                        insert.Parameters.AddWithValue("@RoomNumber", roomNumber);
                        insert.Parameters.AddWithValue("@BookingDate", bookingDate);
                        insert.Parameters.AddWithValue("@RoomType", roomType);
                        insert.Parameters.AddWithValue("@TimeSlot", timeSlot);
                        insert.Parameters.AddWithValue("@CleaningType", cleaningType);
                        insert.Parameters.AddWithValue("@SpecialInstructions", (object)specialInstructions ?? DBNull.Value);

                        insert.ExecuteNonQuery();
                        Console.WriteLine("Booking inserted successfully");
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Booking could not be inserted: " + error.Message);
            }
        }//end StoreBooking

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
