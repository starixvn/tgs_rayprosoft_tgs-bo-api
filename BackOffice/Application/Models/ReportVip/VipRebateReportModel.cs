using System;

namespace TGS.Services.BackOffice.Application.Models.ReportVip;

public record VipRebateReportModel
{
    public DateTimeOffset ReportDate { get; init; }

    public decimal LiveDealerAmount { get; init; }

    public decimal SlotAmount { get; init; }

    public decimal LotteryAmount { get; init; }

    public decimal CardGameAmount { get; init; }

    public decimal SportsAmount { get; init; }

    public decimal VirtualSportsAmount { get; init; }

    public decimal LiveDealerTurnover { get; init; }

    public decimal SlotTurnover { get; init; }

    public decimal LotteryTurnover { get; init; }

    public decimal CardGameTurnover { get; init; }

    public decimal SportsTurnover { get; init; }

    public decimal VirtualSportsTurnover { get; init; }

    public decimal LiveDealerRebatePercentage { get; init; }

    public decimal SlotRebatePercentage { get; init; }

    public decimal LotteryRebatePercentage { get; init; }

    public decimal CardGameRebatePercentage { get; init; }

    public decimal SportsRebatePercentage { get; init; }

    public decimal VirtualSportsRebatePercentage { get; init; }
}