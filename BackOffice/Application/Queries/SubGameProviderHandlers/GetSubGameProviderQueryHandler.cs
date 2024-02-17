using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.SubGameProviderHandlers;

public class GetSubGameProviderQueryHandler(
    ISubGameProviderService service,
    IHttpContextAccessor accessor)
    : Handler<GetSubGameProviderRequest, GetSubGameProviderResponse>(accessor)
{
    private readonly ISubGameProviderService _service = service;

    public override async Task<GetSubGameProviderResponse> Handle(GetSubGameProviderRequest request,
        CancellationToken cancellationToken)
    {
        var subGameProvider = await _service
            .GetSubGameProviderAsync(x => x.Code == request.Code)
            .ConfigureAwait(false);

        return subGameProvider is null
            ? CreateInvalidResponse<GetSubGameProviderResponse>("")
            : new GetSubGameProviderResponse
            {
                Name = subGameProvider.Name,
                Code = subGameProvider.Code,
                Description = subGameProvider.Description,
                GameProviderId = subGameProvider.GameProviderId,
                Localization = subGameProvider.Localization == null
                ? null
                : JsonDocument.Parse(subGameProvider.Localization)
            };
    }
}
