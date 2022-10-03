using System;
using System.Text.Json;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class BrandPaymentRequest
{
    public bool AllPlayers { get; set; }

    public string[] Blacklist { get; set; }

    public Guid BrandId { get; set; }

    public string ChannelName { get; set; }

    public decimal DepositedAmountMax { get; set; }

    public decimal DepositedAmountMin { get; set; }

    public bool DepositedAmountUnlimited { get; set; }

    public JsonDocument ExtendedData { get; set; }

    public decimal HandlingFeeRatePercentile { get; set; }

    public Guid Id { get; set; }

    public bool IdentityAuthentication { get; set; }

    public Guid PaymentMethodChannelId { get; set; }

    public SwitchStatus PaymentStatus { get; set; }

    public bool PhoneAuthentication { get; set; }

    public PlayerListCondition PlayerListCondition { get; set; }

    public string Reason { get; set; }

    public bool ReducePlayerAmount { get; set; }

    public decimal TransferLimitFrom { get; set; }

    public decimal TransferLimitTo { get; set; }

    public Guid[] VipLevelIds { get; set; }

    public string[] Whitelist { get; set; }
}
