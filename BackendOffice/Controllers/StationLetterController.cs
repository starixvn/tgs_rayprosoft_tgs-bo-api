using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ClosedXML.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TGS.Apis.BackendOffice.Controllers.Messages.StationLetter;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Apis.BackendOffice.Services;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StationLetterController(
    ITgsQueries<StationLetter> queries,
    ITgsQueries<Player> playerQueries,
    ITgsCommands<StationLetter> commands,
    IUploadService uploadService,
    IStationLetterService stationLetterService) : BaseContextController
{
    private readonly Dictionary<string, StationLetterStatus> _typeDictionary =
        new()
        {
            { "pass", StationLetterStatus.Published },
            { "reject", StationLetterStatus.Rejected }
        };

    [HttpPut("{id}/status/{status}")]
    [Description("Change an station letter status")]
    public async Task<MessageResponse> ChangeStatusAsync(Guid id, StationLetterStatus status)
    {
        var announcement = await queries.GetSingleAsync(q => q.Id == id);

        if (announcement == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.StationLetter.NotFound);
        }

        announcement.Status = status;
        announcement.UpdatedOn = DateTimeOffset.Now;
        announcement.UpdatedByUsername = Username;

        await commands.UpdateAsync(announcement);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPost]
    [Description("Create a station letter")]
    public async Task<MessageResponse> CreateAsync(
        [FromServices] IJsonSerializationProvider jsonSerializationProvider,
        [FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
        [FromBody] StationLetterRequest request)
    {
        var jsonOptions = jsonOptionsAccessor.Value;

        var stationLetter = new StationLetter
        {
            Id = GuidExtensions.NewSequentialGuid(),
            BrandId = request.BrandId,
            Subject = request.Subject,
            Content = request.Content,
            Localization = request.Localization == null
                ? "{}"
                : jsonSerializationProvider.SerializeToString(
                    request.Localization,
                    jsonOptions.JsonSerializerOptions),
            AssignedAllPlayers = request.AssignedAllPlayers,
            Status = StationLetterStatus.InVerify,
            CreatedByUsername = Username,
            UpdatedByUsername = Username,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        var players = !request.AssignedAllPlayers
            ? await playerQueries.GetQueries()
                .Where(p => p.Status == PlayerStatus.Enabled && p.BrandId == request.BrandId)
                .Where(p => request.PlayerUsernames.Contains(p.Username))
                .Select(x => new StationLetterPlayer
                {
                    PlayerCode = x.PlayerCode,
                    StationLetterId = stationLetter.Id
                })
                .ToArrayAsync()
            : [];

        await stationLetterService.CreateStationLetterAsync(stationLetter, players);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPost("export")]
    [Description("Export player file for example")]
    public async Task<HttpResponseMessage> ExportAsync()
    {
        var wb = await Task.Run(() =>
        {
            var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Player");
            ws.Cell("A1").Value = "Player1";
            ws.Cell("A2").Value = "Player2";

            return workbook;
        });

        return wb.Deliver("Player.xlsx");
    }

    [HttpGet]
    [Description("Get station letters")]
    public Task<PagedResponse<StationLetterModel>> GetListAsync([FromQuery] GetStationLettersRequest request)
    {
        var jsonDocumentOptions = default(JsonDocumentOptions);

        return queries.GetQueries()
            .Where(request.BrandId, x => x.BrandId == request.BrandId)
            .Where(ResellerId, r => r.Brand.Licensee.ResellerId == ResellerId)
            .Where(LicenseeId, l => l.Brand.LicenseeId == LicenseeId)
            .Where(BrandId, l => l.BrandId == BrandId)
            .Where(p => p.Status == request.Status)
            .Where(x => x.Status != StationLetterStatus.Archived)
            .Select(a => new StationLetterModel
            {
                Id = a.Id,
                BrandId = a.BrandId,
                Subject = a.Subject,
                Content = a.Content,
                Localization = JsonDocument.Parse(a.Localization, jsonDocumentOptions),
                Remarks = a.Remarks,
                AssignedAllPlayers = a.AssignedAllPlayers,
                CreatedByUsername = a.CreatedByUsername,
                VerifiedByUsername = a.VerifiedByUsername,
                Status = a.Status.ToString(),
                CreatedOn = a.CreatedOn
            })
            .CreatePagedResponseAsync(request);
    }

    [HttpPut("{id}")]
    [Description("Update a station letter")]
    public async Task<MessageResponse> UpdateAsync(
        [FromServices] IJsonSerializationProvider jsonSerializationProvider,
        [FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
        Guid id,
        [FromBody] StationLetterRequest request)
    {
        var jsonOptions = jsonOptionsAccessor.Value;

        var stationLetter = await queries.GetSingleAsync(p => p.Id == id);

        if (stationLetter == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.StationLetter.NotFound);
        }

        stationLetter.Subject = request.Subject;
        stationLetter.Content = request.Content;

        stationLetter.Localization = request.Localization == null
            ? "{}"
            : jsonSerializationProvider.SerializeToString(
                request.Localization,
                jsonOptions.JsonSerializerOptions);
        stationLetter.UpdatedByUsername = Username;
        stationLetter.UpdatedOn = DateTimeOffset.Now;

        await commands.UpdateAsync(stationLetter);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPost("upload")]
    [Description("Upload player file")]
    public async Task<GetPlayerFromFileResponse> UploadAsync(IFormFile file)
    {
        var uploadedResult = await uploadService.GetUploadedExcelAsync<UploadPlayerModel>(file);
        var users = uploadedResult.Where(p => !string.IsNullOrEmpty(p.Username)).Select(p => p.Username);

        var allPlayers = await playerQueries.GetQueries()
            .Where(p => p.Status == PlayerStatus.Enabled && users.Contains(p.Username))
            .ToArrayAsync();

        var players = from u in users
                      join p in allPlayers on u equals p.Username
                          into tmpU
                      from p in tmpU.DefaultIfEmpty()
                      select new GetPlayerFromFileResponse.PlayerModel
                      {
                          Username = u,
                          Exists = p != null
                      };

        return new GetPlayerFromFileResponse
        {
            Valid = true,
            Players = players.ToArray()
        };
    }

    [HttpPut("{id}/{type}")]
    [Description("Verify a station letter")]
    public async Task<MessageResponse> VerifyAsync(Guid id, string type, [FromBody] VerifyRequest request)
    {
        if (!_typeDictionary.ContainsKey(type))
        {
            return CreateInvalidResponse<MessageResponse>(Resources.StationLetter.InvalidUrl);
        }

        var stationLetter = await queries.GetSingleAsync(p => p.Id == id);

        if (stationLetter == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.StationLetter.NotFound);
        }

        if (stationLetter.Status != StationLetterStatus.InVerify)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.StationLetter.InvalidStatus);
        }

        var targetStatus = _typeDictionary
            .FirstOrDefault(t => t.Key == type);

        stationLetter.Status = targetStatus.Value;
        stationLetter.VerifiedByUsername = Username;
        stationLetter.Remarks = request.Remarks;
        stationLetter.UpdatedByUsername = Username;
        stationLetter.UpdatedOn = DateTimeOffset.Now;

        await commands.UpdateAsync(stationLetter);

        return CreateValidResponse<MessageResponse>();
    }
}
