using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using Tgs.Report.Client.Models.Round;
using TGS.Services.BackOffice.Application.Models.ReportRound;
using TGS.Services.BackOffice.Application.Queries.Game;
using TGS.Services.BackOffice.Domain;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Services.BackOffice.Application.Queries.ReportRoundQueries;

public class
    GetRoundsQuery : IGetService<GetRoundsReportRequest,
        PagedResponse<RoundView>>
{
    private readonly IQueryService<GetAllGamesByBrandIdQuery, GameData> _gameQueryService;
    private readonly IOptions<JsonOptions> _jsonOptionsAccessor;
    private readonly IJsonSerializationProvider _jsonSerializationProvider;

    private readonly IGetService<GetGameRoundListQuery, Tgs.Report.Client.Models.PagedResponse<RoundModel>>
        _queryService;

    public GetRoundsQuery(IOptions<JsonOptions> jsonOptionsAccessor,
        IQueryService<GetAllGamesByBrandIdQuery, GameData> gameQueryService,
        IGetService<GetGameRoundListQuery, Tgs.Report.Client.Models.PagedResponse<RoundModel>> queryService,
        IJsonSerializationProvider jsonSerializationProvider)
    {
        _jsonOptionsAccessor = jsonOptionsAccessor ?? throw new ArgumentNullException(nameof(jsonOptionsAccessor));
        _gameQueryService = gameQueryService ?? throw new ArgumentNullException(nameof(gameQueryService));
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        _jsonSerializationProvider = jsonSerializationProvider ??
            throw new ArgumentNullException(nameof(jsonSerializationProvider));
    }

    public async ValueTask<PagedResponse<RoundView>> GetAsync(
        GetRoundsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var serializerOptions = _jsonOptionsAccessor.Value.JsonSerializerOptions;
        var currentCulture = CultureInfo.CurrentUICulture.Name;

        var query = new GetGameRoundListQuery(
            request.BrandId,
            request.Username,
            request.PlayerCode,
            request.RoundId,
            request.ExternalRoundId,
            request.GameProviderId,
            request.Status,
            request.GameType,
            request.From,
            request.To,
            request.ClosedFrom,
            request.ClosedTo,
            new PageableRequest
            {
                Page = request.Page,
                Rows = request.Rows,
                OrderBy = request.OrderBy,
                SortBy = request.SortBy,
                ShowAll = request.ShowAll
            });

        var records = await _queryService
            .GetAsync(query, cancellationToken)
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

        return new PagedResponse<RoundView>
        {
            Valid = records.Valid,
            Errors = records.Errors,
            Message = records.Message,
            TotalCount = records.TotalCount,
            Items = (from model in records.Items
                     let gameLocalization = allGames.TryGetValue(model.GameId, out var value)
                         ? value.Localization
                         : default
                     let loc = gameLocalization is not null &&
                         gameLocalization.TryGetValue(currentCulture, out var data)
                             ? data
                             : default(GameLocalizationData)
                     select new RoundView
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
                         WinLoss = model.WinLoss
                     }).ToArray()
        };
    }
}