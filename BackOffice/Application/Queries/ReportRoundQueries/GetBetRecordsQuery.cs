using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Round;
using Tgs.ServiceUtility;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.ReportRound;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.ReportRoundQueries;

public class
    GetBetRecordsQuery(IOptions<JsonOptions> jsonOptionsAccessor,
    IQueryService<GetAllGamesByBrandIdQuery, GameData> gameQueryService,
    IReportClient reportClient,
    IJsonSerializationProvider jsonSerializationProvider) : IGetService<GetBetRecordsReportRequest,
        GetBetRecordsReportResponse>
{
    private readonly IQueryService<GetAllGamesByBrandIdQuery, GameData> _gameQueryService = gameQueryService ?? throw new ArgumentNullException(nameof(gameQueryService));
    private readonly IOptions<JsonOptions> _jsonOptionsAccessor = jsonOptionsAccessor ?? throw new ArgumentNullException(nameof(jsonOptionsAccessor));
    private readonly IJsonSerializationProvider _jsonSerializationProvider = jsonSerializationProvider ??
            throw new ArgumentNullException(nameof(jsonSerializationProvider));

    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<GetBetRecordsReportResponse> GetAsync(
        GetBetRecordsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var serializerOptions = _jsonOptionsAccessor.Value.JsonSerializerOptions;
        var currentCulture = CultureInfo.CurrentUICulture.Name;

        var query = new GetBetRecordsRequest
        {
            BrandId = request.BrandId,
            PlayerCode = request.PlayerCode,
            GameProviderId = request.GameProviderId,
            Status = (short?)request.Status,
            GameType = (short?)request.GameType,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var records = await _reportClient.GetBetRecordsAsync(query)
            .ConfigureAwait(false);

        var allGames = await _gameQueryService
            .QueryAsync(new GetAllGamesByBrandIdQuery(request.BrandId), cancellationToken)
            .Select(game => new
            {
                game.GameId,
                Localization = _jsonSerializationProvider
                    .DeserializeFromString<IDictionary<string, GameLocalizationData>>(
                        game.Localization ?? "{}",
                        serializerOptions)
            })
            .ToDictionaryAsync(game => game.GameId, cancellationToken)
            .ConfigureAwait(false);

        return new GetBetRecordsReportResponse
        {
            TotalCount = records.TotalCount,
            Total = new BetRecordTotalView
            {
                BetAmount = records.Total.BetAmount,
                ValidBetAmount = records.Total.ValidBetAmount,
                SettleAmount = records.Total.SettleAmount,
                WinLoss = records.Total.WinLoss
            },
            Items = (from model in records.Datas
                     let gameLocalization = allGames.TryGetValue(model.GameId, out var value)
                         ? value.Localization
                         : default
                     let loc = gameLocalization is not null &&
                         gameLocalization.TryGetValue(currentCulture, out var data)
                             ? data
                             : default
                     select new BetRecordView
                     {
                         Id = model.Id,
                         AdditionalData = model.AdditionalData,
                         BetAmount = model.BetAmount,
                         ClosedOn = model.ClosedOn,
                         Commission = model.Commission,
                         CreatedOn = model.CreatedOn,
                         CurrencyCode = model.CurrencyCode,
                         ExternalClosedOn = model.ExternalClosedOn,
                         ExternalCreatedOn = model.ExternalCreatedOn,
                         ExternalPlayerCode = model.ExternalPlayerCode,
                         ExternalRoundId = model.ExternalRoundId,
                         GameId = model.GameId,
                         GameName = loc?.Name ?? model.GameName,
                         GameProviderCode = model.GameProviderCode,
                         GameProviderName = model.GameProviderName,
                         GameProviderId = model.GameProviderId,
                         PlayerCode = model.PlayerCode,
                         PlatformType = (PlatformType)model.PlatformType,
                         PlayerName = model.PlayerName,
                         SettleAmount = model.SettleAmount,
                         Status = (ClientRoundStatus)model.Status,
                         SubGameProviderName = model.SubGameProviderName,
                         TieBetAdjustment = model.TieBetAdjustment,
                         UpdatedOn = model.UpdatedOn,
                         ValidBetAmount = model.ValidBetAmount,
                         Turnover = model.Turnover,
                         WinLoss = model.WinLoss,
                         EffectiveBettingPercentage = model.EffectiveBettingPercentage,
                         OriginalTurnover = model.OriginalTurnover
                     }).ToArray()
        };
    }
}
