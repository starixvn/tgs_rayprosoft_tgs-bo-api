using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.ServiceUtility;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProvider;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;
using TGS.Services.BackOffice.Application.Queries.GameProviders;

namespace TGS.Apis.BackendOffice.Controllers.GameProvider;

[Route("api/[controller]")]
[ApiController]
public partial class GameProviderController : BaseContextController
{
    private readonly IMediator _mediator;

    public GameProviderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Description("Create a game provider")]
    public Task<MessageResponse> CreateGameProviderAsync(CreateGameProviderRequest request) =>
        _mediator.Send(request);

    [HttpGet("localization/export")]
    [Description("Export game providers localization for example")]
    public async Task<FileStreamResult> ExportGameProvidersLocalizationAsync(
        [FromServices] IQueryService<GetAllSupportLanguagesQuery, string> getAllSupportLangService,
        [FromServices] IQueryService<GetGameProvidersLocalizationQuery, SubGameProviderData> queryService,
        [FromServices] IGetService<CreateSubGameProviderExcelStreamQuery, Stream> createExcelService)
    {
        var allSupportLangs = await getAllSupportLangService
            .QueryAsync(new GetAllSupportLanguagesQuery())
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var subGameProviders = await queryService
            .QueryAsync(new GetGameProvidersLocalizationQuery())
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var fileStream = await createExcelService
            .GetAsync(new CreateSubGameProviderExcelStreamQuery(
                allSupportLangs,
                subGameProviders))
            .ConfigureAwait(false);

        return new FileStreamResult(
            fileStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [HttpGet("{id}")]
    [Description("Get a game provider")]
    public Task<GetGameProviderResponse> GetGameProviderAsync(Guid id) =>
        _mediator.Send(new GetGameProviderRequest(id));

    [HttpGet("brands")]
    [Description("Get game provider Brands")]
    public ValueTask<GameProvidersBrandQueryResult>
       GetGameProviderBrandsAsync(
        [FromServices] IGetService<GameProvidersBrandQueryQuery, GameProvidersBrandQueryResult> getService,
        [FromQuery] Guid? gameProviderId,
        [FromQuery] Guid? brand)
        => getService.GetAsync(new(gameProviderId, brand));

    [HttpGet]
    [Description("Get game providers")]
    public Task<PagedResponse<GameProviderBaseModel>>
        GetGameProvidersAsync([FromQuery] GetGameProvidersRequest request) => _mediator.Send(request);

    [HttpPut("{id}")]
    [Description("Update a game provider")]
    public Task<MessageResponse> UpdateGameProviderAsync(Guid id, UpdateGameProviderRequest request)
    {
        request.Id = id;
        return _mediator.Send(request);
    }

    [HttpPost("localization/upload")]
    [Description("Upload game providers localization file")]
    public async Task<MessageResponse> UploadGameProvidersLocalizationAsync(
        IFormFile file,
        [FromServices] IQueryService<ReadSubGameProviderExcelQuery, UploadSubGameProvidersLocalizationModel> readService,
        [FromServices] IJsonSerializationProvider jsonSerializationProvider,
        [FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
        [FromServices] IQueryService<UpdateGameProvidersLocalizationCommand, string> updateService)
    {
        using var fs = file.OpenReadStream();
        var rows = readService.QueryAsync(new ReadSubGameProviderExcelQuery(fs));

        var serializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;

        var gameProviders = rows
            .Where(row => !string.IsNullOrWhiteSpace(row.Code))
            .Select(row => new SubGameProvidersLocalizationModel
            {
                Code = row.Code,
                Name = row.Name,
                Localization = row.Localization
                    .Select(p => new
                    {
                        p.Key,
                        Value = jsonSerializationProvider.SerializeToString(
                            new
                            {
                                Name = p.Value
                            },
                            serializerOptions)
                    })
                    .ToDictionary(
                        item => item.Key,
                        item => jsonSerializationProvider.DeserializeFromString<JsonDocument>(item.Value))
            });

        var command = new UpdateGameProvidersLocalizationCommand(
            gameProviders.ToEnumerable(),
            Username);

        var diffSubGameProvidersIds = await updateService
            .QueryAsync(command)
            .ToArrayAsync()
            .ConfigureAwait(false);

        return diffSubGameProvidersIds.Any()
            ? new MessageResponse
            {
                Valid = false,
                Message = "GameProviders Not Found",
                Errors = diffSubGameProvidersIds
            }
            : new MessageResponse
            {
                Valid = true,
                Message = "Update Game Providers Localization Success"
            };
    }
}
