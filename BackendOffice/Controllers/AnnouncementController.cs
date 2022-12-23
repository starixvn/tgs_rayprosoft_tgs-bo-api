using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.Announcement;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public partial class AnnouncementController : BaseContextController
{
    private readonly IRedisCacheManager _cacheManager;
    private readonly ITgsCommands<Announcement> _commands;
    private readonly ITgsQueries<Announcement> _queries;

    private readonly Dictionary<string, dynamic> _typeDictionary = new()
    {
        {
            "pass",
            new
            {
                AllowedStatus = AnnounceStatus.InVerify,
                VerifiedStatus = AnnounceStatus.Effective,
                Message = Resources.Announcement.OnlyInVerifyAnnouncementCanBePassed,
                CanRemarks = true
            }
        },
        {
            "reject",
            new
            {
                AllowedStatus = AnnounceStatus.InVerify,
                VerifiedStatus = AnnounceStatus.Rejected,
                Message = Resources.Announcement.OnlyInVerifyAnnouncementCanBeRejected,
                CanRemarks = true
            }
        },
        {
            "invalid",
            new
            {
                AllowedStatus = AnnounceStatus.Effective,
                VerifiedStatus = AnnounceStatus.Invalid,
                Message = Resources.Announcement.OnlyEffectiveAnnouncementCanBeInvalid,
                CanRemarks = false
            }
        }
    };

    public AnnouncementController(
        ITgsQueries<Announcement> queries,
        ITgsCommands<Announcement> commands,
        IRedisCacheManager cacheManager)
    {
        _queries = queries;
        _commands = commands;
        _cacheManager = cacheManager;
    }

    [HttpPut("ChangeOrders", Name = "ChangeAnnouncementOrder")]
    [Description("Change Announcement Order")]
    public async Task<MessageResponse> ChangeOrderAsync(ChangeOrderRequest request)
    {
        var ids = request.Orders.Select(q => q.Id);
        var announcements = await _queries.GetQueries().Where(q => ids.Contains(q.Id)).ToListAsync();
        announcements.ForEach(p => p.DisplayOrder = request.Orders.FirstOrDefault(x => x.Id == p.Id).DisplayOrder);

        await _commands.BatchUpdateAsync(announcements.ToArray());

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPost(Name = "Create")]
    [Description("Create a announcement")]
    public async Task<MessageResponse> CreateAsync(
        [FromServices] IJsonSerializationProvider jsonSerializationProvider,
        [FromServices] IOptions<JsonOptions> optionsAccessor,
        [FromBody] AnnouncementRequest request)
    {
        var serializerOptions = optionsAccessor.Value.JsonSerializerOptions;

        var announcement = new Announcement
        {
            Id = GuidExtensions.NewSequentialGuid(),
            BrandId = request.BrandId,
            DisplayOrder = 99,
            Subject = request.Subject,
            Content = request.Content,
            Localization = request.Localization == null
                ? "{}"
                : jsonSerializationProvider.SerializeToString(
                    request.Localization,
                    serializerOptions),
            From = request.From,
            To = request.To,
            Status = AnnounceStatus.InVerify,
            CreatedByUsername = Username,
            UpdatedByUsername = Username,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now,
            Type = request.Type,
        };

        await _commands.CreateAsync(announcement);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpGet(Name = "GetList")]
    [Description("Get announcements")]
    public Task<PagedResponse<AnnouncementModel>> GetListAsync(
        [FromQuery] GetAnnouncementsRequest request)
    {
        var hasExpiredStatus = request.Status.Contains(AnnounceStatus.Expired);
        var now = DateTimeOffset.Now;

        var serializerOptions = default(JsonDocumentOptions);

        return _queries.GetQueries()
            .Where(request.BrandId, x => x.BrandId == request.BrandId)
            .Where(ResellerId, r => r.Brand.Licensee.ResellerId == ResellerId)
            .Where(LicenseeId, l => l.Brand.LicenseeId == LicenseeId)
            .Where(BrandId, l => l.BrandId == BrandId)
            .Where(x => x.Status != AnnounceStatus.Archived)
            .Where(hasExpiredStatus, p => now > p.To)
            .Where(!hasExpiredStatus,
                p => request.Status.Contains(p.Status) && now < p.To)
            .Where(request.Type, a => a.Type == request.Type)
            .Select(a => new AnnouncementModel
            {
                Id = a.Id,
                BrandId = a.BrandId,
                Subject = a.Subject,
                Content = a.Content,
                Localization = JsonDocument.Parse(a.Localization, serializerOptions),
                Remarks = a.Remarks,
                BrandName = a.Brand.Name,
                CreatedByUsername = a.CreatedByUsername,
                VerifiedByUsername = a.VerifiedByUsername,
                From = a.From,
                To = a.To,
                DisplayOrder = a.DisplayOrder,
                UpdatedOn = a.UpdatedOn,
                Status = now > a.To ? AnnounceStatus.Expired.ToString() : a.Status.ToString(),
                Type = a.Type
            })
            .CreatePagedResponseAsync(request);
    }

    [HttpPut("{id}", Name = "Update")]
    [Description("Update a announcement")]
    public async Task<MessageResponse> UpdateAsync(
        [FromServices] IJsonSerializationProvider jsonSerializationProvider,
        [FromServices] IOptions<JsonOptions> optionsAccessor,
        Guid id,
        [FromBody] AnnouncementRequest request)
    {
        var serializerOptions = optionsAccessor.Value.JsonSerializerOptions;

        var announcement = await _queries.GetSingleAsync(p => p.Id == id);

        if (announcement == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Announcement.NotFound);
        }

        if (announcement.Status is not AnnounceStatus.Effective and not AnnounceStatus.Invalid)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Announcement.CannotUpdateDueToInvalidStatus);
        }

        announcement.Type = request.Type;
        announcement.Subject = request.Subject;
        announcement.Content = request.Content;

        announcement.Localization = request.Localization == null
            ? "{}"
            : jsonSerializationProvider.SerializeToString(
                request.Localization,
                serializerOptions);
        announcement.From = request.From;
        announcement.To = request.To;
        announcement.UpdatedByUsername = Username;
        announcement.UpdatedOn = DateTimeOffset.Now;
        announcement.Status = AnnounceStatus.InVerify;

        await _commands.UpdateAsync(announcement);

        await _cacheManager.ClearCacheAsync(RedisCacheType.Announcements, announcement.BrandId);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPut("{id}/{type}", Name = "Verify")]
    [Description("Verify a announcement")]
    public async Task<MessageResponse> VerifyAsync(Guid id, string type, [FromBody] VerifyRequest request)
    {
        if (!_typeDictionary.ContainsKey(type))
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Announcement.InvalidUrl);
        }

        var announcement = await _queries.GetSingleAsync(p => p.Id == id);

        if (announcement == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Announcement.NotFound);
        }

        foreach (var types in _typeDictionary.Where(types => types.Key == type))
        {
            if (types.Value.AllowedStatus != announcement.Status)
            {
                return CreateInvalidResponse<MessageResponse>(types.Value.Message);
            }

            if (types.Value.CanRemarks)
            {
                announcement.Remarks = request.Remarks;
            }

            announcement.Status = types.Value.VerifiedStatus;
        }

        announcement.UpdatedByUsername = Username;
        announcement.UpdatedOn = DateTimeOffset.Now;

        await _commands.UpdateAsync(announcement);

        return CreateValidResponse<MessageResponse>();
    }
}
