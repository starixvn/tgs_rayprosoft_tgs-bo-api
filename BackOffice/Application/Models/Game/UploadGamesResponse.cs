using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Game;

public class UploadGamesResponse : MessageResponse
{
    public UploadGameModel[] Games { get; set; }
}
