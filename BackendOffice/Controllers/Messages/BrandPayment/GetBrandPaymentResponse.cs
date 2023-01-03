using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class GetBrandPaymentResponse : MessageResponse
{
    public BrandPaymentModel Payment { get; set; }
}
