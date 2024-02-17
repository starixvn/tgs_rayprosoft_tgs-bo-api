using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class GetGameProviderResponse : MessageResponse
{
    public GameProviderModel GameProvider { get; set; }
}
