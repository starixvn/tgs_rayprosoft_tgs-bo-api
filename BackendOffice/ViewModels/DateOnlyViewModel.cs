#nullable enable

using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class DateOnlyViewModel : IParsable<DateOnlyViewModel?>
{
    public DateOnly Date { get; init; }

    public static DateOnlyViewModel? Parse(string s, IFormatProvider? provider)
       => !TryParse(s, provider, out var result)
            ? throw new ArgumentException(@"Could not parse supplied value.", nameof(s))
            : result;

    public static bool TryParse(string? s, IFormatProvider? provider, out DateOnlyViewModel? result)
    {
        if (string.IsNullOrEmpty(s))
        {
            result = null;
            return true;
        }

        if (DateOnly.TryParse(s, out var date))
        {
            result = new DateOnlyViewModel
            {
                Date = date
            };

            return true;
        }

        if (DateTimeOffset.TryParse(s, out var dateTime))
        {
            date = DateOnly.FromDateTime(dateTime.ToOffset(TimeSpan.FromHours(8D)).DateTime);

            result = new DateOnlyViewModel
            {
                Date = date
            };

            return true;
        }

        result = null;

        return false;
    }
}