using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Tgs.ServiceUtility;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Aggregate;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Valhalla.MessageQueue;

namespace TGS.Services.BackOffice.Application.Commands.ReportGenerateAggregate;

public class BrandGameProviderAggregateHandler(
    IMessageSender messageSender,
    IBrandService brandService,
    IBrandGameProviderAggregateAuditLogService brandGameProviderAuditLogService,
    IRedisCacheManager redisCacheManager) : IGetService<GenerateBrandGameProviderAggregateRequest, MessageResponse>
{
    public async ValueTask<MessageResponse> GetAsync(
        GenerateBrandGameProviderAggregateRequest request,
        CancellationToken cancellationToken)
    {
        if (request.StartDate.ToOffset(new(-4, 0, 0)).Day != 1)
        {
            return new MessageResponse
            {
                Valid = false,
                Message = "The date must be number 1 of each month."
            };
        }

        var aggregate = await redisCacheManager.GetSetAllMembersAsync("aggregate", "brandGP")
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
            ? [request.BrandId.Value]
            : await brandService.GetBrandQueries().Select(x => x.Id).ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

        var auditLogId =
            await brandGameProviderAuditLogService.CreateBrandGameProviderAggregateAuditLogAsync(
                    new CreateBrandGameProviderAggregateAuditLogRequest(Guid.Parse(request.UserId),
                        request.BrandId))
                .ConfigureAwait(false);

        _ = await redisCacheManager.SetAddAsync("aggregate",
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

            await messageSender.PublishAsync<ReadOnlyMemory<byte>>("tgs.report.aggregate.brand.game.provider",
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return new MessageResponse { Valid = true };
    }
}
