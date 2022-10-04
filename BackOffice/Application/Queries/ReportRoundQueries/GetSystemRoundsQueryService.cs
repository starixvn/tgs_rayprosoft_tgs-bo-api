using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.Report.Client.Models.Round;
using Tgs.ServiceUtility;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportRound;
using TGS.Services.BackOffice.Application.Queries.Game;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.ReportRoundQueries;

internal class GetSystemRoundsQueryService : IGetService<GetSystemRoundsReportRequest,
        PagedResponse<SystemRoundView>>
{
    private readonly IQueryService<GetAllGamesByBrandIdQuery, GameData> _gameQueryService;
    private readonly IOptions<JsonOptions> _jsonOptionsAccessor;
    private readonly IJsonSerializationProvider _jsonSerializationProvider;

    private readonly IGetService<GetSystemGameRoundListQuery, Tgs.Report.Client.Models.PagedResponse<SystemRoundModel>>
        _queryService;

    public GetSystemRoundsQueryService(IOptions<JsonOptions> jsonOptionsAccessor,
        IQueryService<GetAllGamesByBrandIdQuery, GameData> gameQueryService,
        IGetService<GetSystemGameRoundListQuery, Tgs.Report.Client.Models.PagedResponse<SystemRoundModel>> queryService,
        IJsonSerializationProvider jsonSerializationProvider)
    {
        _jsonOptionsAccessor = jsonOptionsAccessor ?? throw new ArgumentNullException(nameof(jsonOptionsAccessor));
        _gameQueryService = gameQueryService ?? throw new ArgumentNullException(nameof(gameQueryService));
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        _jsonSerializationProvider = jsonSerializationProvider ??
            throw new ArgumentNullException(nameof(jsonSerializationProvider));
    }

    public async ValueTask<PagedResponse<SystemRoundView>> GetAsync(
        GetSystemRoundsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var serializerOptions = _jsonOptionsAccessor.Value.JsonSerializerOptions;
        var currentCulture = CultureInfo.CurrentUICulture.Name;

        var query = new GetSystemGameRoundListQuery(
            request.Username,
            request.PlayerCode,
            request.ExternalRoundId,
            request.RoundId,
            request.ExternalFrom,
            request.ExternalTo,
            request.ExternalClosedFrom,
            request.ExternalClosedTo,
            request.From,
            request.To,
            request.ClosedFrom,
            request.ClosedTo,
            request.Status,
            request.BrandId,
            request.GameProviderId,
            request.SubGameProviderCode,
            request.GameType,
            request.CurrencyCode,
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

        var allGames =
            await Array.Empty<GameData>()
                .ToAsyncEnumerable()
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

        if (request.BrandId.HasValue)
        {
            allGames = await _gameQueryService
                  .QueryAsync(new GetAllGamesByBrandIdQuery(request.BrandId.Value), cancellationToken)
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
        }

        return new PagedResponse<SystemRoundView>
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
                     select new SystemRoundView
                     {
                         PlayerName = model.PlayerName,
                         PlayerCode = model.PlayerCode,
                         ExternalRoundId = model.ExternalRoundId,
                         ExternalCreatedOn = model.ExternalCreatedOn,
                         ExternalClosedOn = model.ExternalClosedOn,
                         Id = model.Id,
                         CreatedOn = model.CreatedOn,
                         ClosedOn = model.ClosedOn,
                         GameProviderName = model.GameProviderName,
                         SubGameProviderName = model.SubGameProviderName,
                         GameName = loc?.Name ?? model.GameName,
                         Status = (ClientRoundStatus)model.Status,
                         CurrencyCode = model.CurrencyCode,
                         BetAmount = model.BetAmount,
                         ValidBetAmount = model.ValidBetAmount,
                         Turnover = model.Turnover,
                         SettleAmount = model.SettleAmount,
                         WinLoss = model.WinLoss,
                         AdditionalData = model.AdditionalData,
                         GameProviderCode = model.GameProviderCode
                     }).ToArray()
        };
    }
}
