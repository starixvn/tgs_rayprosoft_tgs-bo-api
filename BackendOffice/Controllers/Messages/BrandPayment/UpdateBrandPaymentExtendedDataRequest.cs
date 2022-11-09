using System.Text.Json;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class UpdateBrandPaymentExtendedDataRequest
{
    public JsonDocument ExtendedData { get; set; }
}
