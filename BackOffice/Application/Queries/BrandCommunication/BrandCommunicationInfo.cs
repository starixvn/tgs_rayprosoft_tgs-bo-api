using System;

namespace TGS.Services.BackOffice.Application.Queries.BrandCommunication;

public class BrandCommunicationInfo
{
    public string AppId { get; set; }

    public string CommunicationType { get; set; }

    public Guid Id { get; set; }

    public string Phone { get; set; }

    public string QRCodeUrl { get; set; }
}
