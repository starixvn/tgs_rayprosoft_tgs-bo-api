using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class GetPlayerResponse : MessageResponse
{
    public PlayerModel Player { get; set; }
}
