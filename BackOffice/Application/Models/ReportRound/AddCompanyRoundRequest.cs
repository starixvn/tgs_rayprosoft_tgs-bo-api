using System;
using Tgs.GameProviders.Pluggable.ViewModels;

#nullable enable

namespace TGS.Services.BackOffice.Application.Models.ReportRound
{
    public record AddCompanyRoundRequest
    {
        public string ExternalRoundId { get; init; } = default!;
        public Guid GameProviderId { get; init; }
        public string PlayerCode { get; init; } = default!;
        public int GameId { get; init; }
        public DateTimeOffset ExternalCreatedOn { get; init; }
        public DateTimeOffset ExternalClosedOn { get; init; }
        public decimal BetAmount { get; init; }
        public decimal ValidBetAmount { get; init; }
        public decimal SettleAmount { get; init; }
        public decimal Commission { get; init; }
        public decimal Turnover { get; init; }
        public decimal CompanyWinLoss { get; init; }
        public GameRoundClosedClientRoundStatus ClientRoundStatus { get; init; }
        public string? Remark { get; init; }
    }
}
