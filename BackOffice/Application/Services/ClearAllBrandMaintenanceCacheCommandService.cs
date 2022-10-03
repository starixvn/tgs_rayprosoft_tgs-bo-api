using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandService.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Services
{
    internal class ClearAllBrandMaintenanceCacheCommandService : ICommandService<ClearAllBrandMaintenanceCacheCommand>
    {
        private readonly IBrandService _brandService;
        private readonly IMessageSender _messageSender;

        public ClearAllBrandMaintenanceCacheCommandService(
            IBrandService brandService,
            IMessageSender messageSender)
        {
            _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
            _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        }

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
}
