using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Tgs;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.AgentHandlers;

public class GetAgentQueryHandler(
    IAgentService service,
    IAgentDividendService agentDividendService,
    ITgsRepository tgsRepository,
    IHttpContextAccessor accessor) : Handler<GetAgentRequest, GetAgentResponse>(accessor)
{
    private readonly ITgsRepository _tgsRepository = tgsRepository ?? throw new System.ArgumentNullException(nameof(tgsRepository));

    public override async Task<GetAgentResponse> Handle(GetAgentRequest request,
        CancellationToken cancellationToken)
    {
        var agent = await service.GetAgentAsync(x => x.Id == request.Id).ConfigureAwait(false);

        var dividend = await agentDividendService.GetAgentDividends()
            .Where(x => x.AgentId == request.Id && x.SentOn == null)
            .SumAsync(x => x.BeforeDividendAmount ?? x.AgentDividendAmount, cancellationToken).ConfigureAwait(false);

        var playerNumber = (await _tgsRepository.Players.AsQueryable()
            .Where(x => x.AgentId == request.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false)).Length;
        return agent == null
            ? CreateInvalidResponse<GetAgentResponse>(Agent.NotFound)
            : new()
            {
                Valid = true,
                Agent = new()
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
                    Balance = agent.Wallets.FirstOrDefault()?.Balance ?? 0,
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
                    Path = agent.Path
                }
            };
    }
}
