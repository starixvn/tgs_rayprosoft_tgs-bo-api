using System;
using TGS.Apis.BackendOffice.Controllers.Messages.Deposit;

#nullable enable

namespace TGS.Apis.BackendOffice.Controllers.Messages.Withdraw;

public record WithdrawFormViewModel
{
    public decimal? ActualAmount { get; init; }

    public decimal Amount { get; init; }

    public Guid BrandId { get; init; }

    public Guid? BrandPaymentId { get; init; }

    public string ChannelName { get; init; } = default!;

    public DateTimeOffset CreatedOn { get; init; }

    public Guid Id { get; init; }

    public Guid? PaymentMethodId { get; init; }

    public string? UserId { get; init; }

    public string? Remark { get; init; }

    public FlowState Status { get; init; }

    public DateTimeOffset UpdatedOn { get; init; }

    public string? Username { get; init; }

    public string UserType { get; init; } = default!;

    public string? ThirdPartyOrderNo { get; init; }

    public string OrderNo { get; init; } = default!;

    public string? ReviewedUsername { get; init; }

    public DateTimeOffset? ReviewedOn { get; init; }

    public string? PaidUserId { get; init; }

    public string? PaidUsername { get; init; }

    public DateTimeOffset? PaidOn { get; init; }

    public string? AgentName { get; init; } = string.Empty;
}
