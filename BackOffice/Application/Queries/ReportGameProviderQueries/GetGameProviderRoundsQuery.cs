using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.GameProvider;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportGameProviderRound;

namespace TGS.Services.BackOffice.Application.Queries.ReportGameProviderQueries;

public class
    GetGameProviderRoundsQuery : IGetService<GetGameProviderRoundsReportRequest,
        PagedResponse<GameProviderRoundsReportModel>>
{
    private readonly IReportClient _reportClient;

    public GetGameProviderRoundsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<GameProviderRoundsReportModel>> GetAsync(
        GetGameProviderRoundsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetGameProviderRoundsRequest
        {
            From = request.From,
            To = request.To,
            GameProviderId = request.GameProviderId,
            CurrencyCode = request.CurrencyCode,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetGameProviderRoundsAsync(reportRequest).ConfigureAwait(false);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new GameProviderRoundsReportModel
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
        };
    }
}
