using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Domain;

public record BrandPaymentAuditData(
    string ChannelName,
    SwitchStatus PaymentStatus,
    decimal TransferLimitFrom,
    decimal TransferLimitTo,
    PlayerListCondition PlayerListCondition,
    bool AllPlayers,
    bool IdentityAuthentication,
    bool PhoneAuthentication,
    decimal DepositedAmountMin,
    decimal DepositedAmountMax,
    bool DepositedAmountUnlimited);
