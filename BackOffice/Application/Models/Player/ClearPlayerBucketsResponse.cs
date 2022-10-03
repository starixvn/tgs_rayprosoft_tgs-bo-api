using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class ClearPlayerBucketsResponse : MessageResponse
{
    public decimal wageringRequirement { get; set; }
}