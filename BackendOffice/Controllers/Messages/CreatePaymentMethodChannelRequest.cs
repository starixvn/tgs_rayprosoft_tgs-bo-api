using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages;

public class CreatePaymentMethodChannelRequest
{
    public Guid ChannelId { get; init; }

    public Guid MethodId { get; init; }

    public Guid PaymentId { get; init; }
}
