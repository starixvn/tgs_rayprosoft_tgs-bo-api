using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.SubGameProviderHandlers;

public class UpdateSubGameProviderHandler : Handler<UpdateSubGameProviderRequest, MessageResponse>
{
    private readonly ISubGameProviderService _service;

    public UpdateSubGameProviderHandler(ISubGameProviderService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(UpdateSubGameProviderRequest request,
        CancellationToken cancellationToken)
    {
        var subGameProvider = await _service.GetSubGameProviderAsync(x => x.Code == request.Code);
        if (subGameProvider is null)
        {
            return CreateInvalidResponse<MessageResponse>(GameProvider.NotFound);
        }

        await _service.UpdateSubGameProviderAsync(subGameProvider, request);

        return CreateValidResponse<MessageResponse>();
    }
}