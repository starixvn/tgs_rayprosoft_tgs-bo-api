using System;

namespace TGS.Services.BackOffice;

internal class DefaultSystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
