using System;
using System.Text.Json;

namespace TGS.Services.BackOffice.Domain;

public record DepositFormModel(
    Guid Id,
    string PlayerCode,
    string UserName,
    Guid BrandId,
    Guid BrandPaymentId,
    string ChannelName,
    Guid PaymentMethodId,
    decimal Amount,
    string Remark,
    int Status,
    JsonDocument ApplyInfo,
    JsonDocument PaymentReturn,
    DateTimeOffset CreateOn,
    DateTimeOffset UpdateOn);
