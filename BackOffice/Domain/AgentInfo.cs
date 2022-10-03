using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Domain;

public record AgentInfo(
    Guid AgentId,
    string UserName,
    Guid WalletId,
    AgentStatus Status);
