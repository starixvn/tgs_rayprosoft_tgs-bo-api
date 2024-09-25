#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ArticleManagement.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.Articles;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArticleController : BaseContextController
{
    [HttpGet("{brandId:Guid}", Name = "ListDraftForms")]
    public async ValueTask<PagedResponse<DraftListItemViewModel>> ListDraftFormsAsync(
        [FromServices] IGetService<ListDraftFormsQuery, QueryResult<DraftListItem>> queryService,
        Guid brandId,
        [FromQuery] PageableRequest paging)
    {
        var query = new ListDraftFormsQuery(brandId, (paging.Page - 1) * paging.Rows);

        var result = await queryService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);
        return new PagedResponse<DraftListItemViewModel>
        {
            Valid = true,
            TotalCount = result.TotalCount,
            Items = await result.Items.Take(paging.Rows)
                .Select(item => new DraftListItemViewModel
                {
                    Id = item.Id,
                    Description = item.Description,
                    Status = item.Status,
                    Modifier = item.Modifier,
                    PublishDate = item.PublishDate,
                    CreatedOn = item.CreatedOn,
                    UpdatedOn = item.UpdatedOn
                })
                .ToArrayAsync(HttpContext.RequestAborted)
                .ConfigureAwait(false)
        };
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

    [HttpPut("{brandId:Guid}", Name = "CreateNewDraft")]
    public ValueTask<Guid> CreateNewDraftAsync(
        [FromServices] IGetService<CreateNewDraftCommand, Guid> createDraftService,
        Guid brandId,
        [FromBody] CreateDraftFormViewModel viewModel)
    {
        var command = new CreateNewDraftCommand(
            brandId,
            viewModel.Description,
            viewModel.PublishDate,
            viewModel.LocalizedArticles
                .ToDictionary(
                    kv => kv.Key,
                    kv => new Article(
                        kv.Value.Title,
                        kv.Value.Brief,
                        kv.Value.Content,
                        kv.Value.Writer,
                        kv.Value.Banners)),
            Username);

        return createDraftService.GetAsync(command, HttpContext.RequestAborted);
    }

    [HttpDelete("{formId:Guid}", Name = "DeleteDraft")]
    public ValueTask DeleteDraftAsync(
        [FromServices] ICommandService<DeleteDraftCommand> deleteService,
        Guid formId)
    {
        var command = new DeleteDraftCommand(formId);

        return deleteService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpPatch("{formId:Guid}", Name = "UpdateDraft")]
    public ValueTask UpdateDraftAsync(
        [FromServices] ICommandService<UpdateDraftCommand> updateService,
        Guid formId,
        ModifyDraftFormViewModel viewModel)
    {
        var command = new UpdateDraftCommand(
            formId,
            new UpdateDraftForm(
                viewModel.Description,
                viewModel.LocalizedArticles.ToDictionary(
                    kv => kv.Key,
                    kv => new Article(
                        kv.Value.Title,
                        kv.Value.Brief,
                        kv.Value.Content,
                        kv.Value.Writer,
                        kv.Value.Banners)).AsReadOnly()));

        return updateService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpPost("{formId:Guid}:publish", Name = "PublishArticle")]
    public ValueTask PublishArticleAsync(
        [FromServices] ICommandService<PublishArticleCommand> publishService,
        Guid formId,
        [FromBody] DateOnly publishDate)
    {
        var command = new PublishArticleCommand(formId, publishDate);

        return publishService.ExecuteAsync(command, HttpContext.RequestAborted);
    }
}
