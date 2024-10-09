#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate.Clients;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using Tgs.StationLetterService.Client;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.StationLetters;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/station-letter")]
[ApiController]
public class StationLetterV2Controller : ControllerBase
{
    [HttpGet("{brandId}", Name = "GetBrandLetters")]
    public async Task<PagedResponse<StationLetterListItemViewModel>> GetBrandLettersAsync(
        [FromServices] IGetService<GetBrandInfoQuery, BrandInfo?> getBrandService,
        [FromServices] IQueryService<GetBrandLettersQuery, StationLetter> queryService,
        [FromServices] IPlayerServiceClient playerServiceClient,
        Guid brandId,
        [FromQuery(Name = "player")] string? playerCode,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "rows")] int rows = 10)
    {
        var result = await getBrandService.GetAsync(
            new GetBrandInfoQuery(brandId),
            HttpContext.RequestAborted)
            .AsTask()
            .ToAsyncEnumerable()
            .Where(brandInfo => brandInfo is not null)
            .SelectMany(brandInfo => queryService.QueryAsync(
                new GetBrandLettersQuery(brandInfo!.Code),
                HttpContext.RequestAborted))
            .Where(letter => string.IsNullOrEmpty(playerCode) || letter.Sender == playerCode)
            .Buffer(rows)
            .SelectMany(letters => playerServiceClient.QueryPlayerInfoListAsync(
                brandId,
                letters.Select(letter => letter.Sender),
                HttpContext.RequestAborted)
                .ToDictionaryAsync(info => info.PlayerCode, HttpContext.RequestAborted)
                .AsTask()
                .ToAsyncEnumerable()
                .SelectMany(playerMap => letters.ToAsyncEnumerable()
                    .Select(letter => new StationLetterListItemViewModel
                    {
                        Id = letter.Id,
                        Sender = playerMap.TryGetValue(letter.Sender, out var info)
                            ? info.UserName
                            : letter.Sender,
                        Subject = letter.Subject,
                        SentOn = letter.SentOn,
                        Replied = letter.Replied
                    })))
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new PagedResponse<StationLetterListItemViewModel>
        {
            Valid = true,
            TotalCount = result.Length,
            Items = result.Chunk(rows).Skip(page - 1).FirstOrDefault()
        };
    }

    [HttpGet("{brandId}/{letterId}")]
    public async Task<StationLetterViewModel?> GetLetterAsync(
        [FromServices] IGetService<GetBrandInfoQuery, BrandInfo?> getBrandService,
        [FromServices] IGetService<GetLetterByIdQuery, StationLetter?> getLetterService,
        Guid brandId,
        Guid letterId)
    {
        var brandInfo = await getBrandService.GetAsync(
            new GetBrandInfoQuery(brandId),
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (brandInfo == null)
            return null;

        var letter = await getLetterService.GetAsync(
            new GetLetterByIdQuery(letterId),
            HttpContext.RequestAborted).ConfigureAwait(false);

        return letter is null || letter.Receiver != brandInfo.Code
            ? null
            : new StationLetterViewModel
            {
                Id = letter.Id,
                Content = letter.Content,
                HaveRead = letter.HaveRead,
                Quotation = letter.Quotation,
                Receiver = letter.Receiver,
                Replied = letter.Replied,
                Sender = letter.Sender,
                SentOn = letter.SentOn,
                Subject = letter.Subject
            };
    }

    [HttpPut("{brandId}/{letterId}:reply", Name = "ReplyLetter")]
    public async Task ReplyLetterAsync(
        [FromServices] IGetService<GetBrandInfoQuery, BrandInfo?> getBrandService,
        [FromServices] IGetService<GetLetterByIdQuery, StationLetter?> getLetterService,
        [FromServices] ICommandService<ReplyLetterCommand> replyService,
        Guid brandId,
        Guid letterId,
        ReplyStationLetterViewModel viewModel)
    {
        var brandInfo = await getBrandService.GetAsync(
            new GetBrandInfoQuery(brandId),
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (brandInfo is null)
            return;

        var letter = await getLetterService.GetAsync(
            new GetLetterByIdQuery(letterId),
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (letter is null || letter.Receiver != brandInfo.Code)
            return;

        var command = new ReplyLetterCommand(
            brandInfo.Code,
            letter.Sender,
            letterId,
            viewModel.Content,
            HttpContext.User.GetUserName()!);

        await replyService.ExecuteAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);
    }

    [HttpPut("all/{brandId}", Name = "SendAllPlayersLetter")]
    public async Task SendAllPlayersLetterAsync(
        [FromServices] IGetService<GetBrandInfoQuery, BrandInfo?> getBrandService,
        [FromServices] IPlayerServiceClient playerServiceClient,
        [FromServices] ICommandService<SendAllPlayersLetterCommand> sendLetterService,
        Guid brandId,
        [FromBody] CreateAllPlayersStationLetterViewModel viewModel)
    {
        var brandInfo = await getBrandService.GetAsync(
            new GetBrandInfoQuery(brandId),
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (brandInfo == null)
            return;

        var players = await playerServiceClient.QueryPlayerListAsync(
            brandInfo.Id,
            new PlayerListQuery { Status = PlayerStatus.Enabled },
            HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var playerCodes = players.Select(p => p.PlayerCode);

        var command = new SendAllPlayersLetterCommand(
            brandInfo.Code,
            playerCodes,
            viewModel.Subject,
            viewModel.Content,
            HttpContext.User.GetUserName()!);

        await sendLetterService.ExecuteAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);
    }

    [HttpPut("player/{brandId}", Name = "SendLetterByPlayers")]
    public async Task SendLetterAsync(
        [FromServices] IGetService<GetBrandInfoQuery, BrandInfo?> getBrandService,
        [FromServices] ICommandService<SendLetterCommand> sendLetterService,
        Guid brandId,
        [FromBody] CreateStationLetterViewModel viewModel)
    {
        var brandInfo = await getBrandService.GetAsync(
            new GetBrandInfoQuery(brandId),
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (brandInfo == null)
            return;

        var command = new SendLetterCommand(
            brandInfo.Code,
            viewModel.Receivers,
            viewModel.Subject,
            viewModel.Content,
            HttpContext.User.GetUserName()!);

        await sendLetterService.ExecuteAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);
    }
}
