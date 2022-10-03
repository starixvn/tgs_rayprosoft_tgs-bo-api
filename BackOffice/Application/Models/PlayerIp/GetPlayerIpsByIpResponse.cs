using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.PlayerIp;

public class GetPlayerIpsByIpResponse : MessageResponse
{
    public PlayerIpByIpModel[] datas { get; init; }
}
