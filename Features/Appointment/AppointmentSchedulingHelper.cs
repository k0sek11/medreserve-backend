using System.Globalization;

namespace Medreserve.Features.Appointment;

internal static class AppointmentSchedulingHelper
{
    public const int SlotStepMinutes = 15;
    private static readonly DateOnly UnixEpochDate = DateOnly.FromDateTime(DateTime.UnixEpoch);

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

    public static int BuildTimeSlotId(int doctorId, DateOnly date, TimeOnly startTime)
    {
        if (startTime.Minute % SlotStepMinutes != 0 || startTime.Second != 0)
        {
            throw new ArgumentException("Appointment start time must align to 15-minute slots.");
        }

        var dayIndex = date.DayNumber - UnixEpochDate.DayNumber;
        if (dayIndex < 0 || dayIndex > 32767)
        {
            throw new ArgumentOutOfRangeException(nameof(date), "Date is outside supported booking range.");
        }

        var slotIndex = (startTime.Hour * 60 + startTime.Minute) / SlotStepMinutes;
        if (slotIndex < 0 || slotIndex > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(startTime), "Time is outside supported booking range.");
        }

        if (doctorId < 0 || doctorId > 1023)
        {
            throw new ArgumentOutOfRangeException(nameof(doctorId), "Doctor id is outside supported booking range.");
        }

        return (doctorId << 22) | (dayIndex << 7) | slotIndex;
    }

    public static (int DoctorId, DateOnly Date, TimeOnly StartTime) DecodeTimeSlotId(int timeSlotId)
    {
        var doctorId = timeSlotId >> 22;
        var dayIndex = (timeSlotId >> 7) & 0x7FFF;
        var slotIndex = timeSlotId & 0x7F;

        var date = UnixEpochDate.AddDays(dayIndex);
        var startTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(slotIndex * SlotStepMinutes));

        return (doctorId, date, startTime);
    }

    public static bool IsOverlapping(DateTime candidateStart, DateTime candidateEnd, DateTime existingStart, DateTime existingEnd)
    {
        return candidateStart < existingEnd && candidateEnd > existingStart;
    }
}
