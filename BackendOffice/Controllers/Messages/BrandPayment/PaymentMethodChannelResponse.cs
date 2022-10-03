using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class PaymentMethodChannelResponse
{
    public Guid Id { get; init; }

    public Guid PaymentId { get; init; }

    public Guid MethodId { get; init; }

    public Guid ChannelId { get; init; }
}
