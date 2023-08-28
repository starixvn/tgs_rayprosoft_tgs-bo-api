using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

public class GetPlayerIpsByUserNameQuery : PageableRequest
{
    public Guid BrandId { get; init; }

    public string UserName { get; init; }
}
