using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class ClearPlayerBucketsResponse : MessageResponse
{
    public decimal WageringRequirement { get; set; }
}
