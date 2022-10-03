using System;

namespace TGS.Services.BackOffice.Application.Commands.PaymentMethodChannel;

public record CreatePaymentMethodChannelCommand(
    Guid PaymentId,
    Guid MethodId,
    Guid ChannelId);
