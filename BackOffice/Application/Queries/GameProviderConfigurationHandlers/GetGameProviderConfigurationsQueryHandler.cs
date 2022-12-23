using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.GameProviderConfigurationHandlers;

public class GetGameProviderConfigurationsQueryHandler : Handler<GetGameProviderConfigurationsRequest,
    PagedResponse<GameProviderConfigurationModel>>
{
    private readonly IGameProviderConfigurationService _service;

    public GetGameProviderConfigurationsQueryHandler(IGameProviderConfigurationService service,
        IHttpContextAccessor accessor)
        : base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<GameProviderConfigurationModel>> Handle(
        GetGameProviderConfigurationsRequest request,
        CancellationToken cancellationToken) =>
        _service.GetGameProviderConfigurationQueries(q => q.Where(x => x.GameProviderId == request.GameProviderId)
                .Where(ResellerId,
                    r => r.GameProvider.Resellers.Select(x => x.ResellerId).Contains(ResellerId.Value))
                .Where(LicenseeId,
                    l => l.GameProvider.Licensees.Select(x => x.LicenseeId).Contains(LicenseeId.Value))
                .Where(BrandId, b => b.GameProvider.Brands.Select(x => x.BrandId).Contains(BrandId.Value)))
            .Select(x => new GameProviderConfigurationModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ClientId = x.ClientId,
                SecretKey = x.SecretKey,
                Status = x.Status,
                GameProviderId = x.GameProviderId,
                DesktopEndpoint = x.DesktopEndpoint,
                MobileEndpoint = x.MobileEndpoint,
                ConfigurationData = x.ConfigurationData,
                IsDefault = x.IsDefault,
                UpdatedByUsername = x.UpdatedByUsername,
                UpdatedOn = x.UpdatedOn
            }).CreatePagedResponseAsync(request);
}
