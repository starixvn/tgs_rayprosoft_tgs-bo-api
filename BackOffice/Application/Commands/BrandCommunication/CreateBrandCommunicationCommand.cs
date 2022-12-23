using System;

namespace TGS.Services.BackOffice.Application.Commands.BrandCommunication;

public class CreateBrandCommunicationCommand
{
    public string AppId { get; set; }

    public Guid BrandId { get; set; }

    public string CommunicationType { get; set; }

    public string Phone { get; set; }

    public string QRCodeUrl { get; set; }
}
