using System;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class PlayerIdentityFormInfo
{
    public Guid FormId { get; set; }
    public PlayerIdentityReviewState Status { get; set; }
}
