using System.Text.Json;

namespace TGS.Services.BackOffice.Application.Queries.BrandPayment;

public record CreatePaymentCommand(
    string Code,
    string Description,
    JsonDocument ExtendedData);
