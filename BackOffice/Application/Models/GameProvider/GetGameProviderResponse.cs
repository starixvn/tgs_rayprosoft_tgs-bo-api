using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class GetGameProviderResponse : MessageResponse
{
    public GameProviderModel GameProvider { get; set; }
}