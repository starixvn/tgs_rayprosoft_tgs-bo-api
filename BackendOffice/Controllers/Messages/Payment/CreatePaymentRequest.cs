using System.Text.Json;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Payment;

public class CreatePaymentRequest
{
    public string Code { get; set; }

    public string Description { get; set; }

    public JsonDocument ExtendedData { get; set; }
}
