using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class CreateUpdateBrandPaymentResponse : MessageResponse
{
    public string[] DuplicatePlayers { get; set; }
}
