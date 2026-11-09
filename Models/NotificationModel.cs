namespace XISD6329_Task1_CRMS.Models
{
    // What the Notifications page shows
    public class NotificationViewModel
    {
        public string message { get; set; }
        public string notificationType { get; set; }
        public string cleanerName { get; set; }
        public string passkey { get; set; }
        public DateTime? bookingDate { get; set; }
        public DateTime createdAt { get; set; }
    }
}
