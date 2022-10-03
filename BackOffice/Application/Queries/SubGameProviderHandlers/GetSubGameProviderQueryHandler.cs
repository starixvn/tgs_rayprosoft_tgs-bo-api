using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.SubGameProviderHandlers;

public class
    GetSubGameProviderQueryHandler : Handler<GetSubGameProviderRequest, GetSubGameProviderResponse>
{
    private readonly ISubGameProviderService _service;

    public GetSubGameProviderQueryHandler(ISubGameProviderService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override async Task<GetSubGameProviderResponse> Handle(GetSubGameProviderRequest request,
        CancellationToken cancellationToken)
    {
        var subGameProvider = await _service
            .GetSubGameProviderAsync(x => x.Code == request.Code)
            .ConfigureAwait(false);

        if (subGameProvider is null)
        {
            return CreateInvalidResponse<GetSubGameProviderResponse>("");
        }

        return new GetSubGameProviderResponse
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