using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

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

        public string Create_tables()
        {
            //try and catch for error handling
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    
                    connect.Open();

                    //SQL command to create the student table
                    string StudentTable = @"IF OBJECT_ID('dbo.Student','U') IS NULL
                                            BEGIN
                                                CREATE TABLE Student (
                                                StudentID INT PRIMARY KEY IDENTITY (1,1),
                                                Room VARCHAR(10) NOT NULL,
                                                Name VARCHAR(50) NOT NULL,
                                                Email VARCHAR(100) NOT NULL UNIQUE,
                                                Password VARCHAR(75) NOT NULL,
                                                ExternalBookingID VARCHAR(50) NOT NULL UNIQUE
                                                );
                                            END";
                    //SQL command to create the booking table
                    string BookingTable = @"IF OBJECT_ID('dbo.Booking','U') IS NULL
                                            BEGIN
                                                CREATE TABLE Booking(
                                                BookingID INT PRIMARY KEY IDENTITY(1,1),
                                                StudentID INT NOT NULL,
                                                RoomNumber VARCHAR(10) NOT NULL,
                                                BookingDate DATE NOT NULL,
                                                RoomType VARCHAR(20) NOT NULL,
                                                TimeSlot VARCHAR(45) NOT NULL,
                                                CleaningType VARCHAR(30) NOT NULL,
                                                SpecialInstructions VARCHAR(250) NULL,
                                                Status  VARCHAR(20) NOT NULL DEFAULT 'In Progress',
                                                FOREIGN KEY (StudentID) REFERENCES Student(StudentID)
                                                );
                                            END";
                    //SQL command to create the notification table
                    string NotificationTable = @"IF OBJECT_ID('dbo.Notification','U') IS NULL
                                                BEGIN
                                                    CREATE TABLE Notification(
                                                    NotificationID INT PRIMARY KEY IDENTITY(1,1),
                                                    StudentID INT NOT NULL,
                                                    Message VARCHAR(250) NOT NULL,
                                                    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                                                    FOREIGN KEY (StudentID) REFERENCES Student(StudentID)
                                                    );
                                                END";

                    //SQL command to create the cleaning staff table
                    string CleaningStaffTable = @"IF OBJECT_ID('dbo.CleaningStaff','U') IS NULL
                                                BEGIN
                                                    CREATE TABLE CleaningStaff(
                                                    StaffID INT PRIMARY KEY IDENTITY(1,1),
                                                    Name VARCHAR(100) NOT NULL,
                                                    Email VARCHAR(100) NOT NULL UNIQUE,
                                                    Password VARCHAR(75) NOT NULL,
                                                    );
                                                END";

                    //using the 'using function to execute create queries
                    using (SqlCommand createStudentTable = new SqlCommand(StudentTable, connect))
                    {
                        createStudentTable.ExecuteNonQuery();
                        Console.WriteLine("Student table created successfully");
                    }
                    using (SqlCommand createBookingTable = new SqlCommand(BookingTable, connect))
                    {
                        createBookingTable.ExecuteNonQuery();
                        Console.WriteLine("Booking table created successfully");
                    }
                    using (SqlCommand createNotificationTable = new SqlCommand(NotificationTable, connect))
                    {
                        createNotificationTable.ExecuteNonQuery();
                        Console.WriteLine("Notification table created successfully");
                    }
                    using (SqlCommand createCleaningStaffTable = new SqlCommand(CleaningStaffTable, connect))
                    {
                        createCleaningStaffTable.ExecuteNonQuery();
                        Console.WriteLine("CleaningStaff table created successfully");
                    }
                    connect.Close();

                }//end of using connection statement
            }
            catch (Exception error)
            {
                Console.WriteLine("Error creating tables: " + error.Message);
            }
            return "Tables created successfully";
        }//end of Create_Student_table method

        public void StoreStudent(string room, string name, string email, string password)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    //generate a unique external booking id for students
                    string externalBookingId = "EB" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

                    //Query to insert into Student table
                    string insertIntoStudent = @"INSERT INTO Student (Room, Name, Email, Password, ExternalBookingID)
                                         VALUES
                                            ('" + room + "','" + name + "','" + email + "','" + password + "','" + externalBookingId + "');";

                    using (SqlCommand insert = new SqlCommand(insertIntoStudent, connect))
                    {
                        insert.ExecuteNonQuery();
                        Console.WriteLine("Student data inserted successfully");
                    }

                    connect.Close();

                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Student could not be inserted into database" + error.Message);
            }
        }//end of store student

    }

}
