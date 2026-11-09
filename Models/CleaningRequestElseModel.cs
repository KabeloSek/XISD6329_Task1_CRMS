using System.ComponentModel.DataAnnotations;

namespace XISD6329_Task1_CRMS.Models
{
    // "For someone else" form
    public class CleaningRequestElseModel
    {
        [Required]
        public string roomNumber { get; set; }

        [Required(ErrorMessage = "Please select a date")]
        [DataType(DataType.Date)]
        public DateTime bookingDate { get; set; }

        [Required]
        public string roomType { get; set; }

        [Required]
        public string timeSlot { get; set; }

        [Required]
        public string cleaningType { get; set; }

        public string? specialInstructions { get; set; }

        [Required(ErrorMessage = "Enter the External Booking ID of the student you're booking for")]
        public string externalBookingId { get; set; }
    }
}
