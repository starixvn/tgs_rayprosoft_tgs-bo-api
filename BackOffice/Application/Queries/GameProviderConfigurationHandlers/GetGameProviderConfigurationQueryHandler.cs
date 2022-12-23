using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.GameProviderConfigurationHandlers;

public class
    GetGameProviderConfigurationQueryHandler : Handler<GetGameProviderConfigurationRequest,
        GetGameProviderConfigurationResponse>
{
    private readonly IGameProviderConfigurationService _service;

    public GetGameProviderConfigurationQueryHandler(IGameProviderConfigurationService service,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override async Task<GetGameProviderConfigurationResponse> Handle(
        GetGameProviderConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var configuration = await _service.GetGameProviderConfigurationAsync(x => x.Id == request.Id, x => x
            .Where(ResellerId,
                r => r.GameProvider.Resellers.Select(x => x.ResellerId).Contains(ResellerId.Value))
            .Where(LicenseeId,
                l => l.GameProvider.Licensees.Select(x => x.LicenseeId).Contains(LicenseeId.Value))
            .Where(BrandId, b => b.GameProvider.Brands.Select(x => x.BrandId).Contains(BrandId.Value))
        );

        return configuration == null
            ? CreateInvalidResponse<GetGameProviderConfigurationResponse>(GameProviderConfiguration.NotFound)
            : new GetGameProviderConfigurationResponse
            {
                Valid = true,
                GameProviderConfiguration = new GameProviderConfigurationModel
                {
                    Id = configuration.Id,
                    GameProviderId = configuration.GameProviderId,
                    Name = configuration.Name,
                    Description = configuration.Description,
                    ClientId = configuration.ClientId,
                    SecretKey = configuration.SecretKey,
                    Status = configuration.Status,
                    DesktopEndpoint = configuration.DesktopEndpoint,
                    MobileEndpoint = configuration.MobileEndpoint,
                    ConfigurationData = configuration.ConfigurationData,
                    IsDefault = configuration.IsDefault,
                    UpdatedByUsername = configuration.UpdatedByUsername,
                    UpdatedOn = configuration.UpdatedOn
                }
            };
    }
}
