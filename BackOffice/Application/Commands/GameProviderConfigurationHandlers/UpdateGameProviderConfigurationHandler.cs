using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.GameProviderConfigurationHandlers;

public class
    UpdateGameProviderConfigurationHandler : Handler<UpdateGameProviderConfigurationRequest, MessageResponse>
{
    private readonly IGameProviderConfigurationService _service;

    public UpdateGameProviderConfigurationHandler(IGameProviderConfigurationService service,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(UpdateGameProviderConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var configuration = await _service.GetGameProviderConfigurationAsync(x => x.Id == request.Id);

        if (configuration == null)
        {
            return CreateInvalidResponse<MessageResponse>(GameProviderConfiguration.NotFound);
        }

        await _service.UpdateGameProviderConfigurationAsync(configuration, request);

        return CreateValidResponse<MessageResponse>();
    }
}