using System.ComponentModel.DataAnnotations;

namespace XISD6329_Task1_CRMS.Models
{
    public class CleaningRequestModel
    {
        //Required field for room number selected from dropdown
        [Required]
        public string room { get; set; }

        //Required field for student name
        [Required]
        public string name { get; set; }

        [Required]
        public string date { get; set; }

        [Required]
        public string roomType { get; set; }
        [Required]
        public string time { get; set; }
        [Required]
        public string typeOfCleaning { get; set; }
        [Required]
        public string specialInstructions { get; set; }

        //declaration of global connectionString variable
        private string connection = @"Server=(localdb)\south_point_system;Database=southpoint_database;";


    }
}
