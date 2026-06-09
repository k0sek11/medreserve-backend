using System.Globalization;

namespace Medreserve.Features.Appointment;

internal static class AppointmentSchedulingHelper
{
    public const int SlotStepMinutes = 15;

    public static int NormalizeDayOfWeek(int dayOfWeek)
    {
        return dayOfWeek == 0 ? 7 : dayOfWeek;
    }

    public static TimeOnly ParseTime(string value)
    {
        if (TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        throw new FormatException("Time must use HH:mm format.");
    }

    public static DateTime ToDateTime(DateOnly date, TimeOnly time)
    {
        return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, DateTimeKind.Unspecified);
    }

    public static bool IsOverlapping(DateTime candidateStart, DateTime candidateEnd, DateTime existingStart, DateTime existingEnd)
    {
        return candidateStart < existingEnd && candidateEnd > existingStart;
    }
}