using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.AgentHandlers;

public class CreateAgentHandler : Handler<CreateAgentRequest, MessageResponse>
{
    private readonly IAgentService _service;

    public CreateAgentHandler(IAgentService service, IHttpContextAccessor accessor) : base(accessor)
        => _service = service;

    public override async Task<MessageResponse> Handle(
        CreateAgentRequest request,
        CancellationToken cancellationToken)
    {
        if (await _service.ExistsAsync(x =>
                x.BrandId == request.BrandId && x.Username.ToLower().Equals(request.Username.ToLower())))
        {
            return CreateInvalidResponse<MessageResponse>(Agent.NameExisted);
        }

        var parentAgent = request.ParentUsername.IsNotNullOrEmpty()
            ? await _service.GetAgentAsync(x => x.BrandId == request.BrandId &&
                x.Username == request.ParentUsername)
            : null;

        if (request.ParentUsername.IsNotNullOrEmpty())
        {
            if (parentAgent == null)
            {
                return CreateInvalidResponse<MessageResponse>(Agent.ParentAgentNotExists);
            }

            if (parentAgent.Brand.AgentDividendType == AgentDividendType.OneLayer &&
                parentAgent.DividendPercentage < request.DividendPercentage)
            {
                return CreateInvalidResponse<MessageResponse>(Agent
                    .DividendPercentageMustLessThanOrEqualToUpperLayer);
            }
        }

        await _service.CreateAgentAsync(request, parentAgent);

        return CreateValidResponse<MessageResponse>();
    }
}