using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProvider;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.GameProviderHandlers;

public class UpdateGameProviderStatusHandler(IGameProviderService service, IHttpContextAccessor accessor) : Handler<UpdateGameProviderStatusRequest, MessageResponse>(accessor)
{
    private readonly IGameProviderService _service = service;

    public override async Task<MessageResponse> Handle(UpdateGameProviderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var gameProvider = await _service.GetGameProviderAsync(x => x.Id == request.Id);

        await _service.ChangeStatusAsync(gameProvider, request);

        return CreateValidResponse<MessageResponse>();
    }
}
