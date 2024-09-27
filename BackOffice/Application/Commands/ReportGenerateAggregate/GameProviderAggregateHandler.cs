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

public class GameProviderAggregateHandler(
    IMessageSender messageSender,
    IGameProviderService gameProviderService,
    IGameProviderAggregateAuditLogService gameProviderAuditLogService,
    IRedisCacheManager redisCacheManager) : IGetService<GenerateGameProviderAggregateRequest, MessageResponse>
{
    private readonly IGameProviderAggregateAuditLogService _gameProviderAuditLogService = gameProviderAuditLogService;
    private readonly IGameProviderService _gameProviderService = gameProviderService;
    private readonly IMessageSender _messageSender = messageSender;
    private readonly IRedisCacheManager _redisCacheManager = redisCacheManager;

    public async ValueTask<MessageResponse> GetAsync(
        GenerateGameProviderAggregateRequest request,
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

        var aggregate = await _redisCacheManager.GetSetAllMembersAsync("aggregate", "GP")
            .ConfigureAwait(false);

        if (aggregate.Length > 0)
        {
            return new MessageResponse
            {
                Valid = false,
                Message = "waiting for report generation."
            };
        }

        var gameProviderIds = request.GameProviderId is not null
            ? [request.GameProviderId.Value]
            : await _gameProviderService.GetGameProviderQueries().Select(x => x.Id).ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

        var auditLogId =
            await _gameProviderAuditLogService.CreateGameProviderAggregateAuditLogAsync(
                    new CreateGameProviderAggregateAuditLogRequest(Guid.Parse(request.UserId),
                        request.GameProviderId))
                .ConfigureAwait(false);

        _ = await _redisCacheManager.SetAddAsync("aggregate",
            "GP",
            gameProviderIds.Select(x => (RedisValue)x.ToString()).ToArray(),
            TimeSpan.FromMinutes(10)).ConfigureAwait(false);

        foreach (var gameProviderId in gameProviderIds)
        {
            var data = new
            {
                request.StartDate,
                GameProviderId = gameProviderId,
                AuditLogId = auditLogId,
                RedisKey = "aggregate:GP"
            };

            await _messageSender.PublishAsync<ReadOnlyMemory<byte>>("tgs.report.aggregate.game.provider",
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return new MessageResponse { Valid = true };
    }
}
