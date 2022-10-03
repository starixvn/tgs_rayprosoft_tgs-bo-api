using System;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayerDeposit;

public record PlayerDepositReportModel
{
    public DateOnly ReportDate { get; init; }

    public string AgentUserName { get; init; }

    public int FirstDepositCount { get; init; }

    public int NoSecondDepositCount { get; init; }

    public int DepositCount { get; init; }
}
