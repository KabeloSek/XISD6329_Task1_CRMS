namespace XISD6329_Task1_CRMS.Models
{
    // What the booking lists (student My Bookings, cleaner Requests / Bookings) show.
    // bookingId is now Firebase's text id, not a number.
    public class BookingViewModel
    {
        public string bookingId { get; set; }
        public string roomNumber { get; set; }
        public DateTime bookingDate { get; set; }
        public string roomType { get; set; }
        public string timeSlot { get; set; }
        public string cleaningType { get; set; }
        public string specialInstructions { get; set; }
        public string status { get; set; }
        public string cleanerId { get; set; }
        public string cleanerName { get; set; }
    }
}
