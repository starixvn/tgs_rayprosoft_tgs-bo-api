using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Messages;
using Tgs.MessageQueue;
using TGS.Services.BackOffice.Application.Models.Aggregate;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.ReportGenerateAggregate;

public class
    BrandGameProviderAggregateHandler : IGetService<GenerateBrandGameProviderAggregateRequest, MessageResponse>
{
    private readonly IBrandGameProviderAggregateAuditLogService _brandGameProviderAuditLogService;
    private readonly IBrandService _brandService;
    private readonly IMessageSender _messageSender;
    private readonly IRedisCacheManager _redisCacheManager;

    public BrandGameProviderAggregateHandler(
        IMessageSender messageSender,
        IBrandService brandService,
        IBrandGameProviderAggregateAuditLogService brandGameProviderAuditLogService,
        IRedisCacheManager redisCacheManager)
    {
        _brandService = brandService;
        _messageSender = messageSender;
        _brandGameProviderAuditLogService = brandGameProviderAuditLogService;
        _redisCacheManager = redisCacheManager;
    }

    public async ValueTask<MessageResponse> GetAsync(
        GenerateBrandGameProviderAggregateRequest request,
        CancellationToken cancellationToken)
    {
        if (request.StartDate.ToOffset(new(8, 0, 0)).Day != 1)
        {
            return new MessageResponse
            {
                Valid = false,
                Message = "The date must be number 1 of each month."
            };
        }

        var aggregate = await _redisCacheManager.GetSetAllMembersAsync("aggregate", "brandGP")
            .ConfigureAwait(false);

        if (aggregate.Length > 0)
        {
            return new MessageResponse
            {
                Valid = false,
                Message = "waiting for report generation."
            };
        }

        var brandIds = request.BrandId is not null
            ? new[]
            {
                request.BrandId.Value
            }
            : await _brandService.GetBrandQueries().Select(x => x.Id).ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

        var auditLogId =
            await _brandGameProviderAuditLogService.CreateBrandGameProviderAggregateAuditLogAsync(
                    new CreateBrandGameProviderAggregateAuditLogRequest(Guid.Parse(request.UserId),
                        request.BrandId))
                .ConfigureAwait(false);

        await _redisCacheManager.SetAddAsync("aggregate",
            "brandGP",
            brandIds.Select(x => (RedisValue)x.ToString()).ToArray(),
            TimeSpan.FromMinutes(10)).ConfigureAwait(false);

        foreach (var brandId in brandIds)
        {
            var data = new
            {
                request.StartDate,
                BrandId = brandId,
                AuditLogId = auditLogId,
                RedisKey = "aggregate:brandGP"
            };

            await _messageSender.PublishAsync("tgs.report.aggregate.brand.game.provider",
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return new MessageResponse { Valid = true };
    }
}