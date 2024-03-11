using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client;
using Tgs.CashService.Client;
using TGS.Core.DataServices.Tgs;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.AgentHandlers;

public class GetAgentQueryHandler(
    IAgentService service,
    IAgentDividendService agentDividendService,
    ITgsRepository tgsRepository, 
    ICashServiceClient cashServiceClient, 
    IAgentClient agentClient,
    IHttpContextAccessor accessor) : Handler<GetAgentRequest, GetAgentResponse>(accessor)
{
    public override async Task<GetAgentResponse> Handle(GetAgentRequest request,
        CancellationToken cancellationToken)
    {
        var agent = await service.GetAgentAsync(x => x.Id == request.Id).ConfigureAwait(false);

        var dividend = await agentDividendService.GetAgentDividends()
            .Where(x => x.AgentId == request.Id &&
                        x.SentOn == null &&
                        x.ReferencedAgentDividendId == null &&
                        x.BeforeDividendAmount == null)
            .SumAsync(x => x.BeforeDividendAmount ?? x.AgentDividendAmount, cancellationToken)
            .ConfigureAwait(false);

        dividend = dividend > 0 ? 0 : dividend;

        var playerNumber = (await tgsRepository.Players
            .Where(x => x.AgentId == request.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false)).Length;

        var info = await agentClient.GetAgentInfoAsync(new GetAgentInfoRequest(agent.Id), cancellationToken).ConfigureAwait(false);

        var balance = await cashServiceClient.GetBalanceAsync(info!.WalletId!.Value, cancellationToken);

        return new GetAgentResponse
        {
                Valid = true,
                Agent = new AgentModel
                {
                    Id = agent.Id,
                    BrandId = agent.BrandId,
                    Username = agent.Username,
                    RealName = agent.RealName,
                    Status = agent.Status,
                    Type = agent.Type,
                    LevelType = agent.LevelType,
                    InvitationCode = agent.InvitationCode,
                    ParentUsername = agent.ParentAgent?.Username,
                    RegisteredOn = agent.RegisteredOn,
                    Balance = balance,
                    CommissionPercentage = agent.CommissionPercentage,
                    DividendPercentage = agent.DividendPercentage,
                    LiveDealerRebatePercentage = agent.LiveDealerRebatePercentage,
                    SlotRebatePercentage = agent.SlotRebatePercentage,
                    LotteryRebatePercentage = agent.LotteryRebatePercentage,
                    CardGameRebatePercentage = agent.CardGameRebatePercentage,
                    SportsRebatePercentage = agent.SportsRebatePercentage,
                    VirtualSportsRebatePercentage = agent.VirtualSportsRebatePercentage,
                    Email = agent.Email,
                    Phone = agent.Phone,
                    QQ = agent.QQ,
                    Wechat = agent.Wechat,
                    Remarks = agent.Remarks,
                    Banks = agent.Banks.Select(b => new AgentBankModel
                    {
                        Id = b.Id,
                        RealName = b.RealName,
                        BankName = b.BankName,
                        BankAccount = b.BankAccount
                    }).ToArray(),
                    AccumulatedProfitSharing = dividend,
                    PlayerNumber = playerNumber,
                    AgentDividendConditions = [
                        .. agent.AgentDividendConditions.Select(x => new AgentDividendConditionModel
                        {
                            Rank = x.Rank,
                            NetProfit = x.NetProfit,
                            ActiveNumber = x.ActiveNumber,
                            DividendPercentage = x.DividendPercentage
                        }).OrderBy(x => x.Rank),
                    ],
                    Path = agent.Path,
                    NegativeAgentDividendCarryForward = agent.NegativeAgentDividendCarryForward
                }
            };
    }
}
