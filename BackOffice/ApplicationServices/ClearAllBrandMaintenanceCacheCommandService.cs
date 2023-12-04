using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandService.Protos;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Valhalla.MessageQueue;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class ClearAllBrandMaintenanceCacheCommandService(
    IBrandService brandService,
    IMessageSender messageSender) : ICommandService<ClearAllBrandMaintenanceCacheCommand>
{
    private readonly IBrandService _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
    private readonly IMessageSender _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));

    public async ValueTask ExecuteAsync(ClearAllBrandMaintenanceCacheCommand command, CancellationToken cancellationToken = default)
    {
        var brandIds = _brandService.GetBrandQueries().Select(brand => brand.Id).ToArray();

        foreach (var brandId in brandIds)
        {
            var request = new ClearMaintenanceCacheRequest
            {
                BrandId = brandId
            };

            await _messageSender.PublishAsync("tgs.brandservice.maintenance.cache.clear",
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
