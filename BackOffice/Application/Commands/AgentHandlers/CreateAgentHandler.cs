using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tgs.Agent.Client;
using Tgs.BrandCertificate.Clients;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;
using AgentLevelType = Tgs.Agent.Client.Models.Enums.AgentLevelType;
using AgentType = Tgs.Agent.Client.Models.Enums.AgentType;
using CreateAgentRequest = TGS.Services.BackOffice.Application.Models.Agent.CreateAgentRequest;

namespace TGS.Services.BackOffice.Application.Commands.AgentHandlers;

public class CreateAgentHandler(IAgentService service, IAgentClient client, IBrandServiceClient brandServiceClient, IHttpContextAccessor accessor) : Handler<CreateAgentRequest, MessageResponse>(accessor)
{
    public override async Task<MessageResponse> Handle(
        CreateAgentRequest request,
        CancellationToken cancellationToken)
    {
        if (await service.ExistsAsync(x =>
                x.BrandId == request.BrandId && x.Username.ToLower().Equals(request.Username.ToLower())))
        {
            return CreateInvalidResponse<MessageResponse>(Agent.NameExisted);
        }

        var parentAgent = request.ParentUsername.IsNotNullOrEmpty()
            ? await service.GetAgentAsync(x => x.BrandId == request.BrandId &&
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

        var currency = await brandServiceClient.GetAllBrandCurrenciesAsync(cancellationToken)
            .FirstAsync(x => x.BrandId == request.BrandId, cancellationToken);

        _ = await client.CreateAgentAsync(new Tgs.Agent.Client.Models.CreateAgentRequest
        {
            BrandId = BrandId ?? request.BrandId,
            ParentAgentId = parentAgent?.ParentAgentId,
            Username = request.Username,
            RealName = request.RealName ?? "",
            CurrencyCode = currency.CurrencyCode,
            Type = (AgentType)request.Type,
            LevelType = (AgentLevelType)request.LevelType,
            Password = request.Password,
            CardGameRebatePercentage = request.CardGameRebatePercentage,
            CommissionPercentage = request.CommissionPercentage,
            DividendPercentage = request.DividendPercentage,
            LiveDealerRebatePercentage = request.LiveDealerRebatePercentage,
            LotteryRebatePercentage = request.LotteryRebatePercentage,
            SlotRebatePercentage = request.SlotRebatePercentage,
            SportsRebatePercentage = request.SportsRebatePercentage,
            VirtualSportsRebatePercentage = request.VirtualSportsRebatePercentage,
            Remarks = request.Remarks,
            EditorUsername = Username
        }, cancellationToken);

        return CreateValidResponse<MessageResponse>();
    }
}
