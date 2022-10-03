using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.PlayerIp;

public class GetPlayerIpsResponse : MessageResponse
{
    public PlayerIpModel[] datas { get; init; }
}
