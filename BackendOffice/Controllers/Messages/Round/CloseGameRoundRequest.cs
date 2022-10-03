using System;
using Tgs.GameProviders.Pluggable.ViewModels;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Round;

public record CloseGameRoundRequest
{
    public decimal BetAmount { get; init; }

    public GameRoundClosedClientRoundStatus ClientRoundStatus { get; init; }

    public decimal Commission { get; init; }

    public DateTimeOffset ExternalClosedOn { get; init; }

    public string Remark { get; init; } = default!;

    public decimal SettleAmount { get; init; }

    public decimal Turnover { get; init; }

    public decimal ValidBetAmount { get; init; }

    public decimal WinLoss { get; init; }

    public decimal? AdjustAmount { get; init; }
}