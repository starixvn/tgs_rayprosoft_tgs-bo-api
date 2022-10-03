using System;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class PlayerIdentityAuthenticationsModel
{
    public string AttachmentId { get; set; }

    public Guid FormId { get; set; }

    public string IdNumber { get; set; }

    public string PlayerCode { get; set; }

    public string RealName { get; init; }

    public string Remark { get; set; }

    public PlayerIdentityReviewState Status { get; set; }

    public string Username { get; set; }
}
