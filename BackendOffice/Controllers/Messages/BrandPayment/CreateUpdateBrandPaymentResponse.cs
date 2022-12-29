using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class CreateUpdateBrandPaymentResponse : MessageResponse
{
    public string[] DuplicatePlayers { get; set; }
}
