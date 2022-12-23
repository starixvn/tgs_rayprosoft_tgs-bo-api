using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandService.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class ClearBrandMaintenanceCacheCommandService : ICommandService<ClearBrandMaintenanceCacheCommand>
{
    private readonly IMessageSender _messageSender;

    public ClearBrandMaintenanceCacheCommandService(IMessageSender messageSender)
    {
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask ExecuteAsync(ClearBrandMaintenanceCacheCommand command, CancellationToken cancellationToken = default)
    {
        var request = new ClearMaintenanceCacheRequest
        {
            BrandId = command.BrandId
        };

        await _messageSender.PublishAsync("tgs.brandservice.maintenance.cache.clear",
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
