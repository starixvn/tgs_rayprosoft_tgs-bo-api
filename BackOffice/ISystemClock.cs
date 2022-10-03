using System;

namespace TGS.Services.BackOffice;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}
