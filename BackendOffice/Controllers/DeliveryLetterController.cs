#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients.Models;
using Tgs.ServiceUtility;
using Tgs.StationLetterService.Client;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.StationLetters;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/delivery-box")]
[ApiController]
public class DeliveryLetterController : ControllerBase
{
    [HttpGet("{brandId}", Name = "GetBrandDeliveryLetters")]
    public async Task<PagedResponse<DeliveryLetterListItemViewModel>> GetBrandDeliveryLettersAsync(
        [FromServices] IGetService<GetBrandInfoQuery, BrandInfo?> getBrandService,
        [FromServices] IQueryService<GetDeliveryLettersQuery, DeliveryLetter> queryService,
        Guid brandId,
        [FromQuery(Name = "operator")] string? operatorUser,
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
                new GetDeliveryLettersQuery(brandInfo!.Code),
                HttpContext.RequestAborted))
            .Where(letter => string.IsNullOrEmpty(operatorUser)
                || string.Equals(letter.Operator, operatorUser, StringComparison.OrdinalIgnoreCase))
            .Select(letter => new DeliveryLetterListItemViewModel
            {
                Id = letter.Id,
                ReceiverType = letter.ReceiverType.ToString(),
                Subject = letter.Subject,
                SentOn = letter.SentOn,
                Operator = letter.Operator
            })
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new PagedResponse<DeliveryLetterListItemViewModel>
        {
            Valid = true,
            TotalCount = result.Length,
            Items = result.Chunk(rows).Skip(page - 1).FirstOrDefault() ?? []
        };
    }

    [HttpGet("{brandId}/{deliveryId}", Name = "GetDeliveryLetterById")]
    public async Task<DeliveryLetterViewModel?> GetDeliveryLetterAsync(
        [FromServices] IGetService<GetBrandInfoQuery, BrandInfo?> getBrandService,
        [FromServices] IGetService<GetDeliveryLetterByIdQuery, DeliveryLetter?> getLetterService,
        Guid brandId,
        Guid deliveryId)
    {
        var brandInfo = await getBrandService.GetAsync(
            new GetBrandInfoQuery(brandId),
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (brandInfo == null)
            return null;

        var letter = await getLetterService.GetAsync(
            new GetDeliveryLetterByIdQuery(deliveryId),
            HttpContext.RequestAborted).ConfigureAwait(false);

        return letter is null || letter.Sender != brandInfo.Code
            ? null
            : new DeliveryLetterViewModel
            {
                Id = letter.Id,
                Content = letter.Content,
                ReceiverType = letter.ReceiverType.ToString(),
                Receiver = letter.Receiver,
                Sender = letter.Sender,
                SentOn = letter.SentOn,
                Subject = letter.Subject,
                ComeFrom = letter.ComeFrom,
                Operator = letter.Operator
            };
    }
}
