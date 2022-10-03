using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.StationLetter;

public class GetPlayerFromFileResponse : MessageResponse
{
    public PlayerModel[] Players { get; set; }

    public class PlayerModel
    {
        public string Username { get; set; }
        public bool Exists { get; set; }
    }
}
