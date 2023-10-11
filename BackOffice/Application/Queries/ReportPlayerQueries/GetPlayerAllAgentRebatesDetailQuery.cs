using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Player;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportPlayerQueries;

internal class
    GetPlayerAllAgentRebatesDetailQuery : IQueryService<GetPlayerAllAgentRebatesDetailsReportRequest,
        PlayerAllAgentRebateDetailReportModel>
{
    private readonly IReportPlayerAllAgentClient _reportClient;

    public GetPlayerAllAgentRebatesDetailQuery(IReportPlayerAllAgentClient reportClient) =>
        _reportClient = reportClient;

    public IAsyncEnumerable<PlayerAllAgentRebateDetailReportModel> QueryAsync(
        GetPlayerAllAgentRebatesDetailsReportRequest request,
        CancellationToken cancellationToken)
    {
        var reportRequest = new GetPlayerAllAgentRebatesDetailsRequest
        {
            BrandId = request.BrandId, PlayerCode = request.PlayerCode, SettleDateTime = request.SettleDateTime
        };

        return _reportClient
            .GetPlayerAllAgentRebateDetailAsync(reportRequest)
            .Select(x => new PlayerAllAgentRebateDetailReportModel
            {
                Username = x.Username,
                PlayerCode = x.PlayerCode,
                RebateSubTotal = x.RebateSubTotal,
                SportsbookValidBet = x.SportsbookValidBet,
                SportsbookRebate = x.SportsbookRebate,
                VirtualSportsValidBet = x.VirtualSportsValidBet,
                VirtualSportsRebate = x.VirtualSportsRebate,
                LiveDealerValidBet = x.LiveDealerValidBet,
                LiveDealerRebate = x.LiveDealerRebate,
                CardGameValidBet = x.CardGameValidBet,
                CardGameRebate = x.CardGameRebate,
                LotteryValidBet = x.LotteryValidBet,
                LotteryRebate = x.LotteryRebate,
                SlotValidBet = x.SlotValidBet,
                SlotRebate = x.SlotRebate
            });
    }
}