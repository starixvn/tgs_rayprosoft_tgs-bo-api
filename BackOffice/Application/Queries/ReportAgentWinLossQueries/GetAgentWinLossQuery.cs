using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Core.Models.Enums;
using Tgs.Report.Client;
using Tgs.Report.Client.Models;
using Tgs.Report.Client.Models.AgentWinLoss;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentWinLossQueries;

public class
    GetAgentWinLossQuery(
    IReportClient reportClient,
    IAgentService agentService,
    IPlayerService playerService) : IGetService<GetAgentWinLossReportRequest, GetAgentWinLossReportResponse>
{
    public async ValueTask<GetAgentWinLossReportResponse> GetAsync(GetAgentWinLossReportRequest request,
        CancellationToken cancellationToken = default)
    {
        PagedResponse<AgentWinLossModel> response = null;

        switch (string.IsNullOrEmpty(request.Username))
        {
            case true:
                response = await reportClient.GetMasterAgentWinLossAsync(new()
                {
                    BrandId = request.BrandId,
                    From = request.From,
                    To = request.To,
                    Page = request.Page,
                    Rows = request.Rows,
                    OrderBy = (short)request.OrderBy,
                    SortBy = request.SortBy,
                    ShowAll = request.ShowAll
                });
                break;
            case false when !request.DownLine && request.MemberType == AccountType.Agent:
                var agent = await agentService.GetAgentAsync(x => x.BrandId == request.BrandId && x.Username == request.Username);

                if (agent == null)
                {
                    return new()
                    {
                        Valid = true,
                        Items = [],
                        Total = new()
                    };
                }

                response = await reportClient.GetAgentWinLossAsync(new()
                {
                    BrandId = request.BrandId,
                    AgentId = agent.Id,
                    From = request.From,
                    To = request.To,
                    Page = request.Page,
                    Rows = request.Rows,
                    OrderBy = (short)request.OrderBy,
                    SortBy = request.SortBy,
                    ShowAll = request.ShowAll
                });
                break;
            case false when request.DownLine && request.MemberType == AccountType.Agent:
                var downLineAgent = await agentService.GetAgentAsync(x => x.BrandId == request.BrandId && x.Username == request.Username);

                if (downLineAgent == null)
                {
                    return new()
                    {
                        Valid = true,
                        Items = [],
                        Total = new()
                    };
                }

                response = await reportClient.GetDownLineAgentWinLossAsync(new()
                {
                    BrandId = request.BrandId,
                    AgentId = downLineAgent.Id,
                    From = request.From,
                    To = request.To,
                    Page = request.Page,
                    Rows = request.Rows,
                    OrderBy = (short)request.OrderBy,
                    SortBy = request.SortBy,
                    ShowAll = request.ShowAll
                });
                break;
            case false when request.MemberType == AccountType.Player:
                var playerCode = await playerService.GetPlayerQueries()
                    .Where(x => x.BrandId == request.BrandId && x.Username == request.Username)
                    .Select(x => x.PlayerCode)
                    .FirstOrDefaultAsync(cancellationToken);

                if (playerCode == null)
                {
                    return new()
                    {
                        Valid = true,
                        Items = [],
                        Total = new()
                    };
                }

                response = await reportClient.GetAgentPlayerWinLossAsync(new GetPlayerWinLossRequest
                {
                    PlayerCode = playerCode,
                    From = request.From,
                    To = request.To,
                    Page = request.Page,
                    Rows = request.Rows,
                    OrderBy = (short)request.OrderBy,
                    SortBy = request.SortBy,
                    ShowAll = request.ShowAll
                });
                break;
        }

        if (response == null || !response.Valid)
        {
            return new()
            {
                Valid = response?.Valid ?? false,
                Message = response?.Message,
                TotalCount = response?.TotalCount ?? 0,
                Items = [],
                Total = new()
            };
        }

        var pageArray = response.Items.Select(x
                => new AgentWinLossReportModel
                {
                    Id = x.Id,
                    Username = x.Username,
                    MemberType = x.MemberType,
                    RoundCount = x.RoundCount,
                    Turnover = x.Turnover,
                    WinLoss = x.WinLoss,
                    AgentWinLoss = x.AgentWinLoss,
                    CompanyWinLoss = x.CompanyWinLoss,
                    Special = x.Special,
                    AgentSpecial = x.AgentSpecial,
                    CompanySpecial = x.CompanySpecial,
                    AgentTotal = x.AgentTotal,
                    CompanyTotal = x.CompanyTotal,
                    PlatformChargeAmount = x.PlatformChargeAmount
                });

        var total = new AgentWinLossReportTotalModel
        {
            RoundCount = response.Items.Sum(x => x.RoundCount),
            Turnover = response.Items.Sum(x => x.Turnover),
            WinLoss = response.Items.Sum(x => x.WinLoss),
            AgentWinLoss = response.Items.Sum(x => x.AgentWinLoss),
            CompanyWinLoss = response.Items.Sum(x => x.CompanyWinLoss),
            Special = response.Items.Sum(x => x.Special),
            AgentSpecial = response.Items.Sum(x => x.AgentSpecial),
            CompanySpecial = response.Items.Sum(x => x.CompanySpecial),
            AgentTotal = response.Items.Sum(x => x.AgentTotal),
            CompanyTotal = response.Items.Sum(x => x.CompanyTotal),
            PlatformChargeAmount = response.Items.Sum(x => x.PlatformChargeAmount)
        };

        return new()
        {
            Valid = true,
            TotalCount = response.TotalCount,
            Items = pageArray.ToArray(),
            Total = total
        };
    }
}