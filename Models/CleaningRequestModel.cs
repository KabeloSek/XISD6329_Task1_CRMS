using System.ComponentModel.DataAnnotations;

namespace XISD6329_Task1_CRMS.Models
{
    // "For myself" form
    public class CleaningRequestModel
    {
        [Required]
        public string roomNumber { get; set; }

        [Required(ErrorMessage = "Please select a date")]
        public DateTime bookingDate { get; set; }

        [Required]
        public string roomType { get; set; }

        [Required]
        public string timeSlot { get; set; }

        [Required]
        public string cleaningType { get; set; }

        //optional
        public string? specialInstructions { get; set; }
    }
}
