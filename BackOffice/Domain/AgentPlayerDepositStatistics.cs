using System;

namespace TGS.Services.BackOffice.Domain;
public record AgentPlayerDepositStatistics(
    DateOnly ReportDate,
    Guid AgentId,
    int FirstDepositPlayers,
    int DepositPlayers,
    int NoSecondDepositPlayers);
