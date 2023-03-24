using System;

namespace TGS.Services.BackOffice;

public static class DateGenerator
{
    public static DateTimeOffset GetFirstDayOfMonth(DateTimeOffset date)
        => new DateTimeOffset(date.Year, date.Month, 1, 0, 0, 0, date.Offset);

    public static DateTimeOffset GetLastDayOfMonth(DateTimeOffset date)
    {
        var nextMonth = date.AddMonths(1);
        return new DateTimeOffset(nextMonth.Year, nextMonth.Month, 1, 0, 0, 0, nextMonth.Offset).AddDays(-1);
    }

    public static DateTimeOffset GetFirstDayOfWeek(DateTimeOffset date)
        => date.Subtract(new TimeSpan((int) date.DayOfWeek - 1, 0, 0, 0));

    public static DateTimeOffset GetLastDayOfWeek(DateTimeOffset date)
        => date.Subtract(new TimeSpan((int) date.DayOfWeek - 7, 0, 0, 0));

    public static DateTimeOffset GetSpecifyDayOfMonth(DateTimeOffset date, int day)
        => new DateTimeOffset(date.Year, date.Month, day, 0, 0, 0, date.Offset);
}