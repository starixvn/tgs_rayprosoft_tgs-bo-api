using System.Collections.Generic;
using TGS.Apis.BackendOffice.Controllers.Messages.PaymentChannel;
using TGS.Apis.BackendOffice.Controllers.Messages.PaymentMethod;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public record AllPaymentMethodChannelsResponse
{
    public IEnumerable<PaymentMethodChannelResponse> AllData { get; init; }

    public IEnumerable<PaymentMethodModel> Methods { get; init; }

    public IEnumerable<PaymentChannelModel> Channels { get; init; }
}
