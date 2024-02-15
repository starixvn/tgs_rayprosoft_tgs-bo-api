using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;

public class GetGameProviderConfigurationResponse : MessageResponse
{
    public GameProviderConfigurationModel GameProviderConfiguration { get; set; }
}
