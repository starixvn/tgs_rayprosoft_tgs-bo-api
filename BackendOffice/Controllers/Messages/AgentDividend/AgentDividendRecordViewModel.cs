#nullable enable
using System;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.AgentDividend;

public record AgentDividendRecordViewModel(
    Guid Id,
    AgentDividendDistributeStatus Status,
    DateTime From,
    DateTime To,
    string AgentUsername,
    decimal AgentDividendAmount,
    decimal ActualAgentDividendAmount,
    decimal NegativeAgentDividendAmount,
    bool NegativeAgentDividendCarryForward,
    DateTime CreatedOn,
    DateTime UpdatedOn,
    DateTime? ApprovedOn,
    string? Remark);