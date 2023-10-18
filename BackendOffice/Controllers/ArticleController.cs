#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.ArticleManagement.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.Articles;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArticleController : ControllerBase
{
    [HttpGet("{brandId:Guid}", Name = "ListDraftForms")]
    public IAsyncEnumerable<DraftListItemViewModel> ListDraftFormsAsync(
        [FromServices] IQueryService<ListDraftFormsQuery, DraftListItem> queryService,
        Guid brandId)
    {
        var query = new ListDraftFormsQuery(brandId);

        return queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Select(item => new DraftListItemViewModel
            {
                Id = item.Id,
                Description = item.Description,
                Status = item.Status,
                Modifier = item.Modifier,
                PublishDate = item.PublishDate,
                CreatedOn = item.CreatedOn,
                UpdatedOn = item.UpdatedOn
            });
    }

    [HttpGet("detail/{formId:Guid}", Name = "GetDraftForm")]
    public async Task<DraftFormViewModel?> GetDraftFormAsync(
        [FromServices] IGetService<GetDraftFormQuery, DraftForm?> getService,
        Guid formId)
    {
        var query = new GetDraftFormQuery(formId);

        var result = await getService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return result is null
            ? null
            : new DraftFormViewModel
            {
                Id = result.Id,
                Description = result.Description,
                Status = result.Status,
                Modifier = result.Modifier,
                BrandId = result.BrandId,
                CreatedOn = result.CreatedOn,
                UpdatedOn = result.UpdatedOn,
                PublishDate = result.PublishDate,
                LocalizedArticles = result.LocalizedArticles
                    .ToDictionary(
                        kv => kv.Key,
                        kv => new ArticleViewModel
                        {
                            Title = kv.Value.Title,
                            Brief = kv.Value.Brief,
                            Content = kv.Value.Content,
                            Writer = kv.Value.Writer,
                            Banners = kv.Value.Banners
                        })
                    .AsReadOnly()
            };
    }
}
