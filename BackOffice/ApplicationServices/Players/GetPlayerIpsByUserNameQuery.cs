using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

public class GetPlayerIpsByUserNameQuery : PageableRequest
{
    public Guid BrandId { get; init; }

    public string UserName { get; init; }
}
