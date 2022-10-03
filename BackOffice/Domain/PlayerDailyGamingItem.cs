using System;

namespace TGS.Services.BackOffice.Domain;
public record PlayerDailyGamingItem(
    string PlayerCode,
    string UserName,
    Guid AgentId,
    string GpCode,
    string SubGpCode,
    decimal WinLossSummary);
