using System;
using Tgs.Payment.Clients;

namespace TGS.Services.BackOffice.Domain;

public record WithdrawFormModel(
    Guid Id,
    string UserId,
    UserType UserType,
    string UserName,
    Guid BrandId,
    Guid? BrandPaymentId,
    string ChannelName,
    Guid? PaymentMethodId,
    decimal Amount,
    string Remark,
    int Status,
    DateTimeOffset CreatedOn,
    DateTimeOffset UpdatedOn);
