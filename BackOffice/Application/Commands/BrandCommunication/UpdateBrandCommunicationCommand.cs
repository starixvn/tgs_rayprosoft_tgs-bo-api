using System;

namespace TGS.Services.BackOffice.Application.Commands.BrandCommunication;

public class UpdateBrandCommunicationCommand
{
    public string AppdId { get; set; }

    public string CommunicationType { get; set; }

    public Guid Id { get; set; }

    public string Phone { get; set; }

    public string QRCodeUrl { get; set; }
}
