using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.GameProviderHandlers;

public class CreateGameProviderBetLimitHandler : Handler<CreateGameProviderBetLimitRequest, MessageResponse>
{
    private readonly IGameProviderBetLimitService _service;

    public CreateGameProviderBetLimitHandler(IGameProviderBetLimitService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(CreateGameProviderBetLimitRequest request,
        CancellationToken cancellationToken)
    {
        await _service.CreateGameProviderBetLimitAsync(request);

        return CreateValidResponse<MessageResponse>();
    }
}