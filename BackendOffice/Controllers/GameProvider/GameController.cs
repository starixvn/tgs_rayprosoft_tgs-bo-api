using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.Game;
using TGS.Apis.BackendOffice.Services;
using TGS.Services.BackOffice.Application.Models.Game;
using TGS.Services.BackOffice.Application.Queries.Game;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.GameProvider;

[Route("api/[controller]")]
[ApiController]
public class GameController(IMediator mediator, IUploadService uploadService) : BaseContextController
{
    [HttpPost]
    [Description("Create games")]
    public Task<MessageResponse> CreateGamesAsync(CreateGamesRequest request) =>
        mediator.Send(request);

    [AllowAnonymous]
    [HttpGet("export/{gameProviderId}")]
    [Description("Export games")]
    public async Task<IActionResult> ExportGamesAsync(
        [FromServices] IQueryService<GetAllGamesByGameProviderIdQuery, GameInfo> queryService,
        [FromRoute] Guid gameProviderId)
    {
        var query = new GetAllGamesByGameProviderIdQuery(gameProviderId);
        var games = await queryService.QueryAsync(query)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var memoryStream = new MemoryStream();

        using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());

            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Games"
            });

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;

            var row = new Row();
            row.Append(
                new Cell
                {
                    CellValue = new CellValue("Name"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("Code"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("RuleUrl"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("ExternalId"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("Type"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("PlatformType"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("HasTurnoverOnTieBet"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("EndpointPath"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("Description"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("SubGame"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("AvailableToBrand"),
                    DataType = CellValues.String
                },

                new Cell
                {
                    CellValue = new CellValue("Html5"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("AvailableCurrencies"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("SubGpCode"),
                    DataType = CellValues.String
                }
            );
            _ = sheetData.AppendChild(row);

            foreach (var game in games)
            {
                row = new Row();
                row.Append(
                    new Cell
                    {
                        CellValue = new CellValue(game.Name),
                        DataType = CellValues.String
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.Code),
                        DataType = CellValues.String
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.RuleUrl),
                        DataType = CellValues.String
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.ExternalId),
                        DataType = CellValues.String
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.Type.ToString()),
                        DataType = CellValues.String
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.PlatformType.ToString()),
                        DataType = CellValues.String
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.HasTurnoverOnTieBet),
                        DataType = CellValues.Boolean
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.EndpointPath),
                        DataType = CellValues.String
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.Description),
                        DataType = CellValues.String
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.SubGame),
                        DataType = CellValues.Boolean
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.AvailableToBrand),
                        DataType = CellValues.Boolean
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.Html5),
                        DataType = CellValues.Boolean
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.AvailableCurrencies),
                        DataType = CellValues.String
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.SubGameProviderCode),
                        DataType = CellValues.String
                    });

                _ = sheetData.AppendChild(row);
            }
        }

        _ = memoryStream.Seek(0, SeekOrigin.Begin);
        return File(
            memoryStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Games.xlsx");
    }

    [HttpGet("localization/export")]
    [Description("Export game localization for example")]
    public async Task<FileStreamResult> ExportGamesLocalizationAsync(
        [FromServices] IGetService<GetGameLocalizationQuery, GameLocalizationInfo> getLocalizationQueryService,
        [FromQuery] ExportGameLocalizationRequest request
    )
    {
        var query = new GetGameLocalizationQuery(request.GameProviderCode);
        var info = await getLocalizationQueryService.GetAsync(query).ConfigureAwait(false);

        var memoryStream = new MemoryStream();

        using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());

            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = request.GameProviderCode
            });

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            var row = new Row();
            row.Append(
                new Cell
                {
                    CellValue = new CellValue("Id"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("Code"),
                    DataType = CellValues.String
                }
            );

            foreach (var item in info.Languages)
            {
                row.Append(
                    new Cell
                    {
                        CellValue = new CellValue(item.Replace("-", "")),
                        DataType = CellValues.String
                    }
                );
            }

            _ = sheetData.AppendChild(row);

            foreach (var game in info.Games)
            {
                row = new Row();
                row.Append(
                    new Cell
                    {
                        CellValue = new CellValue(game.GameId),
                        DataType = CellValues.String
                    },
                    new Cell
                    {
                        CellValue = new CellValue(game.GameCode),
                        DataType = CellValues.String
                    }
                );
                if (game.Localization != null)
                {
                    foreach (var item in info.Languages)
                    {
                        var localization = JsonDocument.Parse(game.Localization);
                        var isExist = localization.RootElement.TryGetProperty(item, out var lang);
                        if (isExist)
                        {
                            row.Append(
                                new Cell
                                {
                                    CellValue = new CellValue(lang.GetProperty("name").ToString()),
                                    DataType = CellValues.String
                                }
                            );
                        }
                    }
                }

                _ = sheetData.AppendChild(row);
            }
        }

        _ = memoryStream.Seek(0, SeekOrigin.Begin);
        return new FileStreamResult(
            memoryStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [HttpGet]
    [Description("Get games")]
    public Task<PagedResponse<GameModel>>
        GetGameProvidersAsync([FromQuery] GetGamesRequest request) => mediator.Send(request);

    [HttpPut("{id}")]
    [Description("Update a game")]
    public Task<MessageResponse> UpdateGameAsync(int id, UpdateGameRequest request)
    {
        request.Id = id;
        return mediator.Send(request);
    }

    [HttpPost("upload")]
    [Description("Upload games")]
    public async Task<IActionResult> UploadGamesAsync([FromForm] UploadGamesRequest request)
    {
        var response = await mediator.Send(request).ConfigureAwait(false);

        return response.Valid ? Ok(response) : (IActionResult)StatusCode(400, response);
    }

    [HttpPost("localization/upload")]
    [Description("Upload game localization file")]
    public async Task<MessageResponse> UploadGamesLocalizationAsync(
        IFormFile file,
        [FromServices]
        IGetService<UpdateGameLocalizationQuery, UpdateGameLocationInfo> updateGameLocalizationQueryService)
    {
        var uploadedResult = await uploadService.GetUploadedExcelAsync<UploadGameLocalizationModel>(file);

        var games = uploadedResult
            .Where(p => !string.IsNullOrEmpty(p.Id))
            .Select(x => new GameLocalizationModel
            {
                Id = int.Parse(x.Id),
                Code = x.Code,
                Localization = new Dictionary<string, JsonDocument>
                {
                    { "en-US", JsonDocument.Parse("{ \"name\": \"" + x.EnUS + "\" }") },
                    { "zh-CN", JsonDocument.Parse("{ \"name\": \"" + x.ZhCN + "\" }") },
                    { "zh-TW", JsonDocument.Parse("{ \"name\": \"" + x.ZhTW + "\" }") },
                    { "id-ID", JsonDocument.Parse("{ \"name\": \"" + x.IdID + "\" }") },
                    { "th-TH", JsonDocument.Parse("{ \"name\": \"" + x.ThTH + "\" }") },
                    { "vi-VN", JsonDocument.Parse("{ \"name\": \"" + x.ViVN + "\" }") },
                    { "ko-KR", JsonDocument.Parse("{ \"name\": \"" + x.KoKR + "\" }") },
                    { "ja-JP", JsonDocument.Parse("{ \"name\": \"" + x.JaJP + "\" }") }
                }
            })
            .ToList();

        var query = new UpdateGameLocalizationQuery
        {
            Games = games,
            EditorUsername = Username
        };

        var info = await updateGameLocalizationQueryService.GetAsync(query).ConfigureAwait(false);

        var diffGames = info.DiffGameIds;

        return diffGames.Length != 0
            ? new MessageResponse
            {
                Valid = false,
                Message = "Games Not Found",
                Errors = diffGames.Select(x => x.ToString()).ToArray()
            }
            : new MessageResponse
            {
                Valid = true,
                Message = "Update Games Localization Success"
            };
    }
}
