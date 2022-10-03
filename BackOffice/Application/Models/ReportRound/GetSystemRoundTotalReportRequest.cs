using System;
using TGS.Core.Models.Enums;

#nullable enable

namespace TGS.Services.BackOffice.Application.Models.ReportRound
{
    public class GetSystemRoundTotalReportRequest
    {
        public Guid? BrandId { get; init; }

        public DateTimeOffset? ClosedFrom { get; init; }

        public DateTimeOffset? ClosedTo { get; init; }

        public string? CurrencyCode { get; init; }

        public DateTimeOffset? ExternalClosedFrom { get; init; }

        public DateTimeOffset? ExternalClosedTo { get; init; }

        public DateTimeOffset? ExternalFrom { get; init; }

        public string? ExternalRoundId { get; init; }

        public DateTimeOffset? ExternalTo { get; init; }

        public DateTimeOffset? From { get; init; }

        public Guid GameProviderId { get; init; }

        public GameType? GameType { get; init; }

        public string? PlayerCode { get; init; }

        public Guid? RoundId { get; init; }

        public ClientRoundStatus? Status { get; init; }

        public string? SubGameProviderCode { get; init; }

        public DateTimeOffset? To { get; init; }

        public string? Username { get; init; }
    }
}
