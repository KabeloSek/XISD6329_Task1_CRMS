using System.Globalization;
using XISD6329_Task1_CRMS.Models;

namespace XISD6329_Task1_CRMS.Services
{
    // Turns the Firebase objects into the view models the Razor pages use
    public static class BookingMapper
    {
        public static DateTime ParseDate(string? text)
            => DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d : DateTime.MinValue;

        public static BookingViewModel ToView(Booking b) => new BookingViewModel
        {
            bookingId = b.Id,
            roomNumber = b.RoomNumber,
            bookingDate = ParseDate(b.BookingDate),
            roomType = b.RoomType,
            timeSlot = b.TimeSlot,
            cleaningType = b.CleaningType,
            specialInstructions = b.SpecialInstructions,
            status = b.Status,
            cleanerId = b.CleanerId,
            cleanerName = string.IsNullOrEmpty(b.CleanerName) ? "Not yet assigned" : b.CleanerName
        };

        public static NotificationViewModel ToView(AppNotification n) => new NotificationViewModel
        {
            message = n.Message,
            notificationType = n.Type,
            cleanerName = n.CleanerName,
            passkey = n.Passkey,
            bookingDate = string.IsNullOrEmpty(n.BookingDate) ? null : ParseDate(n.BookingDate),
            createdAt = DateTimeOffset.FromUnixTimeMilliseconds(n.CreatedAt).LocalDateTime
        };
    }
}
