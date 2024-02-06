#nullable enable

using System;
using System.Text.Json;
using TGS.Apis.BackendOffice.Controllers.Messages.Deposit;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Withdraw;

public class WithdrawFormDetailViewModel
{
    public decimal? ActualAmount { get; init; }

    public decimal Amount { get; init; }

    public JsonDocument? ApplyInfo { get; init; }

    public JsonDocument BankInfo { get; init; } = default!;

    public Guid BrandId { get; init; }

    public Guid? BrandPaymentId { get; init; }

    public string ChannelName { get; init; } = default!;

    public DateTimeOffset CreatedOn { get; init; }

    public decimal HandlingFee { get; init; }

    public Guid Id { get; init; }

    public string? Note { get; init; }

    public string OrderNo { get; init; } = default!;

    public DateTimeOffset? PaidOn { get; init; }

    public string? PaidUserId { get; init; }

    public string? PaidUserName { get; init; }

    public Guid? PaymentMethodId { get; init; }

    public JsonDocument? PaymentReturn { get; init; }

    public string? RealName { get; init; }

    public string? Remark { get; init; }

    public string? PlayerRemark { get; set; }

    public DateTimeOffset? ReviewedOn { get; init; }

    public string? ReviewedUserName { get; init; }

    public FlowState Status { get; init; }

    public string? ThirdPartyOrderNo { get; init; }

    public DateTimeOffset UpdatedOn { get; init; }

    public string? UserId { get; init; }

    public string? UserName { get; init; }

    public string UserType { get; init; } = default!;
}
