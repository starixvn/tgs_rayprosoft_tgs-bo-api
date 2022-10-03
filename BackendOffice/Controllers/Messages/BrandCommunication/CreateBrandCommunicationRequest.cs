using System;

#nullable enable

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandCommunication;

public class CreateBrandCommunicationRequest
{
    public string? AppId { get; set; }

    public Guid BrandId { get; set; }

    public string? Phone { get; set; }

    public string? QRCodeUrl { get; set; }

    public string Type { get; set; } = default!;
}
