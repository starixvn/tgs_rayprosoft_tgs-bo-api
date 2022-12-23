using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.BrandGameProvider;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportBrandGameProviderRound;

namespace TGS.Services.BackOffice.Application.Queries.ReportBrandGameProviderQueries;

public class
    GetBrandGameProviderRoundsQuery : IGetService<GetBrandGameProviderRoundsReportRequest,
        GetBrandGameProviderRoundsReportResponse>
{
    private readonly IReportClient _reportClient;

    public GetBrandGameProviderRoundsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<GetBrandGameProviderRoundsReportResponse> GetAsync(
        GetBrandGameProviderRoundsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetBrandGameProviderRoundsRequest
        {
            From = request.From,
            To = request.To,
            BrandId = request.BrandId,
            GameProviderCode = request.GameProviderCode,
            GameType = (short?)request.GameType,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetBrandGameProviderRoundsAsync(reportRequest);

        return new()
        {
            Datas = new PagedResponse<BrandGameProviderRoundsReportModel>
            {
                TotalCount = response.Datas.TotalCount,
                Items = response.Datas.Items.Select(x => new BrandGameProviderRoundsReportModel
                {
                    GameProviderId = x.GameProviderId,
                    GameProviderName = x.GameProviderName,
                    CurrencyCode = x.CurrencyCode,
                    TransactionCount = x.TransactionCount,
                    BetAmount = x.BetAmount,
                    ValidBetAmount = x.ValidBetAmount,
                    OriginalTurnover = x.OriginalTurnover,
                    CompanyWinLoss = x.CompanyWinLoss
                }).ToArray()
            },
            Totals = response.Totals.Select(x => new BrandGameProviderRoundTotalReportModel
            {
                CurrencyCode = x.CurrencyCode,
                TransactionCount = x.TransactionCount,
                BetAmount = x.BetAmount,
                ValidBetAmount = x.ValidBetAmount,
                OriginalTurnover = x.OriginalTurnover,
                CompanyWinLoss = x.CompanyWinLoss
            }).ToArray()
        };
    }
}
