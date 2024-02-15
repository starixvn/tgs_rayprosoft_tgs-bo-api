using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.BetLimit;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BetLimitHandlers;

public class UpdateBetLimitHandler(
    IBetLimitService service,
    IHttpContextAccessor accessor)
    : Handler<UpdateBetLimitRequest, MessageResponse>(accessor)
{
    private readonly IBetLimitService _service = service;

    public override async Task<MessageResponse> Handle(UpdateBetLimitRequest request,
        CancellationToken cancellationToken)
    {
        var betLimit = await _service.GetBetLimitAsync(q => q.Id == request.Id);

        if (betLimit == null)
        {
            return CreateInvalidResponse<MessageResponse>(BetLimit.NotFound);
        }

        await _service.UpdateBetLimitAsync(betLimit, request);

        return CreateValidResponse<MessageResponse>();
    }
}
