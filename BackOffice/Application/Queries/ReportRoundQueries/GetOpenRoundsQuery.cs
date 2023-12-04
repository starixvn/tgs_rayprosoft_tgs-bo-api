using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Round;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportRound;

namespace TGS.Services.BackOffice.Application.Queries.ReportRoundQueries;

public class
    GetOpenRoundsQuery(IReportClient reportClient) : IGetService<GetOpenRoundsReportRequest,
        PagedResponse<OpenRoundReportModel>>
{
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<OpenRoundReportModel>> GetAsync(
        GetOpenRoundsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetOpenRoundsRequest
        {
            ExternalRoundId = request.ExternalRoundId,
            GameProviderId = request.GameProviderId,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetOpenRoundsAsync(reportRequest);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new OpenRoundReportModel
            {
                Id = x.Id,
                AdditionalData = x.AdditionalData,
                BetAmount = x.BetAmount,
                BrandCode = x.BrandCode,
                BrandName = x.BrandName,
                ClosedOn = x.ClosedOn,
                Commission = x.Commission,
                CreatedOn = x.CreatedOn,
                CurrencyCode = x.CurrencyCode,
                ExternalClosedOn = x.ExternalClosedOn,
                ExternalCreatedOn = x.ExternalCreatedOn,
                ExternalPlayerCode = x.ExternalPlayerCode,
                ExternalRoundId = x.ExternalRoundId,
                GameId = x.GameId,
                GameName = x.GameName,
                GameProviderCode = x.GameProviderCode,
                GameProviderId = x.GameProviderId,
                PlatformType = (PlatformType)x.PlatformType,
                PlayerCode = x.PlayerCode,
                PlayerName = x.PlayerName,
                PlayerType = (PlayerType)x.PlayerType,
                SettleAmount = x.SettleAmount,
                Status = (ClientRoundStatus)x.Status,
                SubGameProviderName = x.SubGameProviderName,
                TieBetAdjustment = x.TieBetAdjustment,
                UpdatedOn = x.UpdatedOn,
                ValidBetAmount = x.ValidBetAmount,
                WinLoss = x.WinLoss
            }).ToArray()
        };
    }
}
