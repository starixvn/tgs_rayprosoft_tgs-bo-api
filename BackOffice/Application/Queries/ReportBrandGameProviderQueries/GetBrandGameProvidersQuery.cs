using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TGS.Core.Models.Messages;
using Tgs.Report.Client;
using TGS.Services.BackOffice.Application.Models.ReportBrandGameProviderRound;
using Tgs.ServiceUtility;
using GameType = Tgs.Report.Client.Enum.GameType;
using GetBrandGameProvidersRequest = Tgs.Report.Client.Models.BrandGameProvider.GetBrandGameProvidersRequest;

namespace TGS.Services.BackOffice.Application.Queries.ReportBrandGameProviderQueries;

public class
    GetBrandGameProvidersQuery(IReportClient reportClient) : IGetService<GetBrandGameProvidersReportRequest,
    GetBrandGameProvidersReportResponse>
{
    public async ValueTask<GetBrandGameProvidersReportResponse> GetAsync(
        GetBrandGameProvidersReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetBrandGameProvidersRequest
        {
            From = request.From,
            To = request.To,
            BrandId = request.BrandId,
            GameProviderCode = request.GameProviderCode,
            GameType = (GameType?)request.GameType,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await reportClient.GetBrandGameProvidersAsync(reportRequest);

        return new GetBrandGameProvidersReportResponse
        {
            Datas = new PagedResponse<BrandGameProvidersReportModel>
            {
                TotalCount = response.Datas.TotalCount,
                Items = response.Datas.Items.Select(x => new BrandGameProvidersReportModel
                    {
                        GameProviderId = x.GameProviderId,
                        GameProviderName = x.GameProviderName,
                        CurrencyCode = x.CurrencyCode,
                        TransactionCount = x.TransactionCount,
                        PlayerCount = x.PlayerCount,
                        BetAmount = x.BetAmount,
                        ValidBetAmount = x.ValidBetAmount,
                        OriginalTurnover = x.OriginalTurnover,
                        CompanyWinLoss = x.CompanyWinLoss
                    })
                    .ToArray()
            },
            Total = new BrandGameProviderTotalReportModel
            {
                CurrencyCode = response.Total.CurrencyCode,
                TransactionCount = response.Total.TransactionCount,
                BetAmount = response.Total.BetAmount,
                ValidBetAmount = response.Total.ValidBetAmount,
                OriginalTurnover = response.Total.OriginalTurnover,
                CompanyWinLoss = response.Total.CompanyWinLoss,
                PlayerCount = response.Total.PlayerCount
            }
        };
    }
}