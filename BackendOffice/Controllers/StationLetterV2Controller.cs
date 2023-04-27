#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate.Clients;
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
        Guid brandId,
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
            .Select(letter => new StationLetterListItemViewModel
            {
                Id = letter.Id,
                Sender = letter.Sender,
                Subject = letter.Subject,
                SentOn = letter.SentOn
            })
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new PagedResponse<StationLetterListItemViewModel>
        {
            Valid = true,
            TotalCount = result.Length,
            Items = result.Chunk(rows).Skip(page - 1).FirstOrDefault()
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
            viewModel.Content);

        await replyService.ExecuteAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);
    }

    [HttpPut("{brandId}", Name = "SendLetter")]
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
            viewModel.Content);

        await sendLetterService.ExecuteAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);
    }
}
