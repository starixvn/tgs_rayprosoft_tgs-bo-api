using System;

namespace TGS.Services.BackOffice.Domain;
public record PlayerDailyRegisterItem(
    string PlayerCode,
    string UserName,
    Guid AgentId,
    DateTime RegisteredOn,
    DateTime? LastLoginOn,
    int? LoginCount);
