using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class GetBrandPaymentAuditLogResponse : MessageResponse
{
    public PaymentAuditLogModel[] AuditLogs { get; set; }
}

public class PaymentAuditLogModel
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public string BeforeChannelName { get; set; }
    public string AfterChannelName { get; set; }
    public SwitchStatus BeforePaymentStatus { get; set; }
    public SwitchStatus AfterPaymentStatus { get; set; }
    public decimal BeforeTransferLimitFrom { get; set; }
    public decimal AfterTransferLimitFrom { get; set; }
    public decimal BeforeTransferLimitTo { get; set; }
    public decimal AfterTransferLimitTo { get; set; }
    public PlayerListCondition BeforePlayerListCondition { get; set; }
    public PlayerListCondition AfterPlayerListCondition { get; set; }
    public bool BeforeAllPlayers { get; set; }
    public bool AfterAllPlayers { get; set; }
    public bool BeforeIdentityAuthentication { get; set; }
    public bool AfterIdentityAuthentication { get; set; }
    public bool BeforePhoneAuthentication { get; set; }
    public bool AfterPhoneAuthentication { get; set; }
    public bool BeforeDepositedAmountUnlimited { get; set; }
    public bool AfterDepositedAmountUnlimited { get; set; }
    public decimal BeforeDepositedAmountMin { get; set; }
    public decimal AfterDepositedAmountMin { get; set; }
    public decimal BeforeDepositedAmountMax { get; set; }
    public decimal AfterDepositedAmountMax { get; set; }
    public string Reason { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public string CreatedByUsername { get; set; }
    public BrandPaymentOperation Operation { get; set; }
}