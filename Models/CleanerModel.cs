using System.Data.SqlClient;

namespace XISD6329_Task1_CRMS.Models
{
    public class BookingViewModel
    {
        public int bookingId { get; set; }
        public string roomNumber { get; set; }
        public DateTime bookingDate { get; set; }
        public string roomType { get; set; }
        public string timeSlot { get; set; }
        public string cleaningType { get; set; }
        public string specialInstructions { get; set; }
        public string status { get; set; }
        public int? cleanerId { get; set; }
        public string cleanerName { get; set; }
    }

    public class CleanerModel
    {
        private string connection = @"Server=(localdb)\south_point_system;Database=southpoint_database;Trusted_Connection=True;";

        //all cleaners see the same open pool of unaccepted requests
        public List<BookingViewModel> GetOpenRequests()
        {
            List<BookingViewModel> bookings = new List<BookingViewModel>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT * FROM Booking WHERE Status='Pending' ORDER BY BookingDate";

                    using (SqlCommand command = new SqlCommand(query, connect))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookings.Add(new BookingViewModel
                            {
                                bookingId = (int)reader["BookingID"],
                                roomNumber = reader["RoomNumber"].ToString(),
                                bookingDate = (DateTime)reader["BookingDate"],
                                roomType = reader["RoomType"].ToString(),
                                timeSlot = reader["TimeSlot"].ToString(),
                                cleaningType = reader["CleaningType"].ToString(),
                                specialInstructions = reader["SpecialInstructions"] == DBNull.Value ? "" : reader["SpecialInstructions"].ToString(),
                                status = reader["Status"].ToString()
                            });
                        }
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error fetching open requests: " + error.Message);
            }

            return bookings;
        }//end GetOpenRequests

        //bookings this specific cleaner has accepted, split by status in the controller/view
        public List<BookingViewModel> GetCleanerBookings(int cleanerId)
        {
            List<BookingViewModel> bookings = new List<BookingViewModel>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT * FROM Booking WHERE CleanerID=@CleanerID ORDER BY BookingDate";

                    using (SqlCommand command = new SqlCommand(query, connect))
                    {
                        command.Parameters.AddWithValue("@CleanerID", cleanerId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                bookings.Add(new BookingViewModel
                                {
                                    bookingId = (int)reader["BookingID"],
                                    roomNumber = reader["RoomNumber"].ToString(),
                                    bookingDate = (DateTime)reader["BookingDate"],
                                    roomType = reader["RoomType"].ToString(),
                                    timeSlot = reader["TimeSlot"].ToString(),
                                    cleaningType = reader["CleaningType"].ToString(),
                                    specialInstructions = reader["SpecialInstructions"] == DBNull.Value ? "" : reader["SpecialInstructions"].ToString(),
                                    status = reader["Status"].ToString()
                                });
                            }
                        }
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error fetching cleaner bookings: " + error.Message);
            }

            return bookings;
        }//end GetCleanerBookings

        public bool AcceptBooking(int bookingId, int cleanerId, string cleanerName)
        {
            bool success = false;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    //only accept if it's still Pending — stops two cleaners accepting the same booking
                    string update = @"UPDATE Booking SET Status='In Progress', CleanerID=@CleanerID
                                       WHERE BookingID=@BookingID AND Status='Pending'";

                    int studentId = 0;
                    DateTime bookingDate = default;

                    using (SqlCommand fetch = new SqlCommand("SELECT StudentID, BookingDate FROM Booking WHERE BookingID=@BookingID", connect))
                    {
                        fetch.Parameters.AddWithValue("@BookingID", bookingId);
                        using (SqlDataReader reader = fetch.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                studentId = (int)reader["StudentID"];
                                bookingDate = (DateTime)reader["BookingDate"];
                            }
                        }
                    }

                    using (SqlCommand command = new SqlCommand(update, connect))
                    {
                        command.Parameters.AddWithValue("@CleanerID", cleanerId);
                        command.Parameters.AddWithValue("@BookingID", bookingId);
                        int rows = command.ExecuteNonQuery();
                        success = rows > 0;
                    }

                    if (success && studentId != 0)
                    {
                        string insertNotification = @"INSERT INTO Notification (StudentID, Message, NotificationType, BookingID, CleanerName, BookingDate, CreatedAt)
                                                       VALUES
                                                       (@StudentID, @Message, 'Accepted', @BookingID, @CleanerName, @BookingDate, GETDATE())";

                        using (SqlCommand notify = new SqlCommand(insertNotification, connect))
                        {
                            notify.Parameters.AddWithValue("@StudentID", studentId);
                            notify.Parameters.AddWithValue("@Message", cleanerName + " has accepted your cleaning request.");
                            notify.Parameters.AddWithValue("@BookingID", bookingId);
                            notify.Parameters.AddWithValue("@CleanerName", cleanerName);
                            notify.Parameters.AddWithValue("@BookingDate", bookingDate);
                            notify.ExecuteNonQuery();
                        }
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error accepting booking: " + error.Message);
            }

            return success;
        }//end AcceptBooking

        //verifies the passkey the cleaner was given by the student before marking the job done
        public bool CompleteBooking(int bookingId, string enteredPasskey, string cleanerName)
        {
            bool success = false;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string storedPasskey = "";
                    int studentId = 0;
                    DateTime bookingDate = default;

                    using (SqlCommand fetch = new SqlCommand("SELECT StudentID, Passkey, BookingDate FROM Booking WHERE BookingID=@BookingID", connect))
                    {
                        fetch.Parameters.AddWithValue("@BookingID", bookingId);
                        using (SqlDataReader reader = fetch.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                studentId = (int)reader["StudentID"];
                                storedPasskey = reader["Passkey"].ToString();
                                bookingDate = (DateTime)reader["BookingDate"];
                            }
                        }
                    }

                    if (storedPasskey != enteredPasskey)
                    {
                        return false; //wrong passkey — do not mark complete
                    }

                    string update = @"UPDATE Booking SET Status='Completed' WHERE BookingID=@BookingID";
                    using (SqlCommand command = new SqlCommand(update, connect))
                    {
                        command.Parameters.AddWithValue("@BookingID", bookingId);
                        success = command.ExecuteNonQuery() > 0;
                    }

                    if (success)
                    {
                        string insertNotification = @"INSERT INTO Notification (StudentID, Message, NotificationType, BookingID, CleanerName, BookingDate, CreatedAt)
                                                       VALUES
                                                       (@StudentID, @Message, 'Completed', @BookingID, @CleanerName, @BookingDate, GETDATE())";

                        using (SqlCommand notify = new SqlCommand(insertNotification, connect))
                        {
                            notify.Parameters.AddWithValue("@StudentID", studentId);
                            notify.Parameters.AddWithValue("@Message", "Your cleaning has been completed by " + cleanerName + ".");
                            notify.Parameters.AddWithValue("@BookingID", bookingId);
                            notify.Parameters.AddWithValue("@CleanerName", cleanerName);
                            notify.Parameters.AddWithValue("@BookingDate", bookingDate);
                            notify.ExecuteNonQuery();
                        }
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error completing booking: " + error.Message);
            }

            return success;
        }//end CompleteBooking

        public List<BookingViewModel> GetStudentBookings(int studentId)
        {
            List<BookingViewModel> bookings = new List<BookingViewModel>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT b.*, c.Name AS CleanerName FROM Booking b
                              LEFT JOIN CleaningStaff c ON b.CleanerID = c.StaffID
                              WHERE b.StudentID=@StudentID
                              ORDER BY b.BookingDate DESC";

                    using (SqlCommand command = new SqlCommand(query, connect))
                    {
                        command.Parameters.AddWithValue("@StudentID", studentId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                bookings.Add(new BookingViewModel
                                {
                                    bookingId = (int)reader["BookingID"],
                                    roomNumber = reader["RoomNumber"].ToString(),
                                    bookingDate = (DateTime)reader["BookingDate"],
                                    roomType = reader["RoomType"].ToString(),
                                    timeSlot = reader["TimeSlot"].ToString(),
                                    cleaningType = reader["CleaningType"].ToString(),
                                    status = reader["Status"].ToString(),
                                    cleanerId = reader["CleanerID"] == DBNull.Value ? (int?)null : (int)reader["CleanerID"],
                                    cleanerName = reader["CleanerName"] == DBNull.Value ? "Not yet assigned" : reader["CleanerName"].ToString()
                                });
                            }
                        }
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error fetching student bookings: " + error.Message);
            }

            return bookings;
        }
    }
}