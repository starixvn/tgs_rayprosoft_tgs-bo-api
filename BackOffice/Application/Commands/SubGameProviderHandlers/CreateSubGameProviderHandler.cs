using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.SubGameProviderHandlers;

public class CreateSubGameProviderHandler : Handler<CreateSubGameProviderRequest, MessageResponse>
{
    private readonly ISubGameProviderService _service;

    public CreateSubGameProviderHandler(ISubGameProviderService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(CreateSubGameProviderRequest request,
        CancellationToken cancellationToken)
    {
        if (await _service.ExistsAsync(x => x.Code == request.Code))
        {
            return CreateInvalidResponse<MessageResponse>(GameProvider.CodeExisted);
        }

        await _service.CreateSubGameProviderAsync(request);

        return CreateValidResponse<MessageResponse>();
    }
}