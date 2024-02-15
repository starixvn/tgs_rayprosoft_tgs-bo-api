using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.BetLimit;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BetLimitHandlers;

public class CreateBetLimitHandler(
    IBetLimitService service,
    IHttpContextAccessor accessor) 
    : Handler<CreateBetLimitRequest, MessageResponse>(accessor)
{
    private readonly IBetLimitService _service = service;

    public override async Task<MessageResponse> Handle(CreateBetLimitRequest request,
        CancellationToken cancellationToken)
    {
        await _service.CreateBetLimitAsync(request);

        return CreateValidResponse<MessageResponse>();
    }
}
