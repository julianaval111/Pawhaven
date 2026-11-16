
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;

namespace Pawhaven.Services
{
    public class BookingAvailabilityService
    {
        private readonly ApplicationDbContext _context;

        private const int MaximumSimultaneousBookings = 2;
        private const int OpeningHour = 9;
        private const int ClosingHour = 18;
        private const int SlotIntervalMinutes = 30;

        public BookingAvailabilityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsAvailableAsync(
            DateTime appointmentDate,
            int durationMinutes,
            int? excludedBookingId = null)
        {
            if (durationMinutes <= 0)
                return false;

            var start = appointmentDate;
            var end = start.AddMinutes(durationMinutes);

            if (start.Date != end.Date ||
                start.TimeOfDay < new TimeSpan(OpeningHour, 0, 0) ||
                end.TimeOfDay > new TimeSpan(ClosingHour, 0, 0))
            {
                return false;
            }

            var dayStart = start.Date;
            var dayEnd = dayStart.AddDays(1);

            var bookings = await _context.Bookings
                .Include(b => b.GroomingService)
                .Where(b =>
                    b.Status == "Approved" &&
                    b.BookingId != excludedBookingId &&
                    b.AppointmentDate >= dayStart &&
                    b.AppointmentDate < dayEnd)
                .ToListAsync();

            var events = new List<(DateTime Time, int Delta)>
            {
                (start, 1),
                (end, -1)
            };

            foreach (var booking in bookings)
            {
                if (booking.GroomingService == null)
                    continue;

                var existingStart = booking.AppointmentDate;
                var existingEnd = existingStart.AddMinutes(
                    booking.GroomingService.DurationMinutes);

                if (existingStart < end && existingEnd > start)
                {
                    events.Add((existingStart, 1));
                    events.Add((existingEnd, -1));
                }
            }

            var activeBookings = 0;

            foreach (var appointmentEvent in events
                .OrderBy(e => e.Time)
                .ThenBy(e => e.Delta))
            {
                activeBookings += appointmentEvent.Delta;

                if (activeBookings > MaximumSimultaneousBookings)
                    return false;
            }

            return true;
        }

        public async Task<List<DateTime>> GetAvailableSlotsAsync(
            DateTime date,
            int durationMinutes)
        {
            var availableSlots = new List<DateTime>();

            if (durationMinutes <= 0)
                return availableSlots;

            var philippinesTimeZone =
                TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

            var nowInPhilippines = TimeZoneInfo.ConvertTime(
                DateTimeOffset.UtcNow,
                philippinesTimeZone).DateTime;

            var startTime = date.Date.AddHours(OpeningHour);
            var closingTime = date.Date.AddHours(ClosingHour);

            for (var slot = startTime;
                 slot.AddMinutes(durationMinutes) <= closingTime;
                 slot = slot.AddMinutes(SlotIntervalMinutes))
            {
                if (slot <= nowInPhilippines)
                    continue;

                var available = await IsAvailableAsync(
                    slot,
                    durationMinutes);

                if (available)
                    availableSlots.Add(slot);
            }

            return availableSlots;
        }
    }
}
