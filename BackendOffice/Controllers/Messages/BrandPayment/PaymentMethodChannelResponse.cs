using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class PaymentMethodChannelResponse
{
    public Guid ChannelId { get; init; }

    public Guid Id { get; init; }

    public Guid MethodId { get; init; }

    public Guid PaymentId { get; init; }
}
