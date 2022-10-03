using System;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class PlayerIdentityFormModel
{
    public DateTimeOffset CreatedOn { get; set; }

    public string Data { get; set; }

    public Guid Id { get; set; }

    public string IdNumber { get; set; }

    public string RealName { get; init; }

    public string Remark { get; set; }

    public DateTimeOffset? ReviewedOn { get; set; }

    public string ReviewedUserName { get; set; }

    public PlayerIdentityReviewState Status { get; set; }
}
