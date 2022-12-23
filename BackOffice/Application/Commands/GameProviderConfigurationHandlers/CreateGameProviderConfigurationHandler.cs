using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.GameProviderConfigurationHandlers;

public class
    CreateGameProviderConfigurationHandler : Handler<CreateGameProviderConfigurationRequest, MessageResponse>
{
    private readonly IGameProviderConfigurationService _service;

    public CreateGameProviderConfigurationHandler(IGameProviderConfigurationService service,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(CreateGameProviderConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        await _service.CreateGameProviderConfigurationAsync(request);

        return CreateValidResponse<MessageResponse>();
    }
}
