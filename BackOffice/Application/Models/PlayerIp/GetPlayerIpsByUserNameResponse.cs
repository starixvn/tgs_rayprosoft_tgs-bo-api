using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.PlayerIp;

public class GetPlayerIpsByUserNameResponse : MessageResponse
{
    public PlayerIpByUserNameModel[] Datas { get; init; }
}
