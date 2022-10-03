using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.GameProviderHandlers;

public class UpdateGameProviderBetLimitHandler : Handler<UpdateGameProviderBetLimitRequest, MessageResponse>
{
    private readonly IGameProviderBetLimitService _service;

    public UpdateGameProviderBetLimitHandler(IGameProviderBetLimitService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(UpdateGameProviderBetLimitRequest request,
        CancellationToken cancellationToken)
    {
        var getLimit = await _service.GetGameProviderBetLimitAsync(x => x.Id == request.Id);

        if (getLimit == null)
        {
            return CreateInvalidResponse<MessageResponse>(GameProviderBetLimit.NotFound);
        }

        await _service.UpdateGameProviderBetLimitAsync(getLimit, request);

        return CreateValidResponse<MessageResponse>();
    }
}