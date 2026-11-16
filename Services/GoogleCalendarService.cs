
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

namespace Pawhaven.Services
{
    public class GoogleCalendarService
    {
        private readonly CalendarService _calendarService;
        private readonly string _calendarId;

        public GoogleCalendarService()
        {
            var credentialsJson =
                Environment.GetEnvironmentVariable("GOOGLE_CALENDAR_CREDENTIALS_JSON");

            _calendarId =
                Environment.GetEnvironmentVariable("GOOGLE_CALENDAR_ID")
                ?? throw new InvalidOperationException(
                    "Google Calendar ID is not configured.");

            if (string.IsNullOrWhiteSpace(credentialsJson))
            {
                throw new InvalidOperationException(
                    "Google Calendar credentials are not configured.");
            }

            var serviceAccountCredential =
                CredentialFactory
                    .FromJson<ServiceAccountCredential>(credentialsJson);

            var credential = serviceAccountCredential
                .ToGoogleCredential()
                .CreateScoped(CalendarService.Scope.Calendar);

            _calendarService = new CalendarService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Pawhaven"
                });
        }

        public async Task<string> CreateEventAsync(
            string petName,
            string serviceName,
            DateTime appointmentDate,
            int durationMinutes)
        {
            var philippinesTimeZone =
                TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

            var startTime = DateTime.SpecifyKind(
                appointmentDate,
                DateTimeKind.Unspecified);

            var endTime = startTime.AddMinutes(durationMinutes);

            var calendarEvent = new Event
            {
                Summary = $"Pawhaven Grooming - {petName}",
                Description =
                    $"Grooming Service: {serviceName}\nPet: {petName}",

                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(
                        startTime,
                        philippinesTimeZone.GetUtcOffset(startTime)),
                    TimeZone = "Asia/Manila"
                },

                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(
                        endTime,
                        philippinesTimeZone.GetUtcOffset(endTime)),
                    TimeZone = "Asia/Manila"
                }
            };

            var request = _calendarService.Events.Insert(
                calendarEvent,
                _calendarId);

            var createdEvent = await request.ExecuteAsync();

            return createdEvent.Id;
        }

        public async Task DeleteEventAsync(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return;
            }

            await _calendarService.Events.Delete(
                _calendarId,
                eventId).ExecuteAsync();
        }
    }
}
