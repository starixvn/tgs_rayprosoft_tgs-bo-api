using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Vip;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportVip;

namespace TGS.Services.BackOffice.Application.Queries.ReportVipQueries;

public class
    GetVipRebateReportQuery(IReportClient reportClient) : IGetService<GetVipRebateReportReportRequest,
        PagedResponse<VipRebateReportModel>>
{
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<VipRebateReportModel>> GetAsync(
        GetVipRebateReportReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetVipRebateReportRequest
        {
            BrandId = request.BrandId,
            Username = request.Username,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetVipRebateReportAsync(reportRequest);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new VipRebateReportModel
            {
                ReportDate = x.ReportDate,
                LiveDealerAmount = x.LiveDealerAmount,
                SlotAmount = x.SlotAmount,
                LotteryAmount = x.LotteryAmount,
                CardGameAmount = x.CardGameAmount,
                SportsAmount = x.SportsAmount,
                VirtualSportsAmount = x.VirtualSportsAmount,
                LiveDealerTurnover = x.LiveDealerTurnover,
                SlotTurnover = x.SlotTurnover,
                LotteryTurnover = x.LotteryTurnover,
                CardGameTurnover = x.CardGameTurnover,
                SportsTurnover = x.SportsTurnover,
                VirtualSportsTurnover = x.VirtualSportsTurnover,
                LiveDealerRebatePercentage = x.LiveDealerRebatePercentage,
                SlotRebatePercentage = x.SlotRebatePercentage,
                LotteryRebatePercentage = x.LotteryRebatePercentage,
                CardGameRebatePercentage = x.CardGameRebatePercentage,
                SportsRebatePercentage = x.SportsRebatePercentage,
                VirtualSportsRebatePercentage = x.VirtualSportsRebatePercentage
            }).ToArray()
        };
    }
}
