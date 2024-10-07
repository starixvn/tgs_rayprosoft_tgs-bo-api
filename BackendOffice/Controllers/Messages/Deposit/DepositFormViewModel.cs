using System;
using System.Text.Json;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Deposit;

public record DepositFormViewModel
{
    public Guid Id { get; init; }

    public string PlayerCode { get; init; }

    public string Username { get; init; }

    public Guid BrandId { get; init; }

    public Guid? BrandPaymentId { get; init; }

    public Guid? PaymentMethodId { get; init; }

    public string ChannelName { get; init; }

    public FlowState Status { get; init; }

    public decimal Amount { get; init; }
    public decimal? ActualAmount { get; init; }

    public string Remark { get; init; }

    public DateTimeOffset CreatedOn { get; init; }

    public DateTimeOffset UpdatedOn { get; init; }

    public string ThirdPartyOrderNo { get; init; }

    public string OrderNo { get; init; }

    public string ReviewedUsername { get; init; }

    public string PromotionName { get; init; }

    public decimal? Bonus { get; init; }

    public decimal HandlingFee { get; init; }

    public string? PromotionType { get; init; }
    
    public string AgentName { get; init; }

    public JsonDocument? PromotionDetail { get; init; }
}
