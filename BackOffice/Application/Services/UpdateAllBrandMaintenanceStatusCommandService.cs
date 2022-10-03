using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Services
{
    internal class UpdateAllBrandMaintenanceStatusCommandService : ICommandService<UpdateAllBrandMaintenanceStatusCommand>
    {
        private readonly IBrandService _service;

        public UpdateAllBrandMaintenanceStatusCommandService(IBrandService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async ValueTask ExecuteAsync(UpdateAllBrandMaintenanceStatusCommand command, CancellationToken cancellationToken = default)
        {
            var brands = await _service
                .GetBrandQueries(brand => brand.Where(b => b.Status == BrandStatus.Active))
                .ToArrayAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            foreach (var brand in brands)
            {
                var request = new UpdateBrandStatusRequest(brand.Id, BrandStatus.Maintenance)
                {
                    EditorUsername = command.EditorUsername
                };

                await _service
                    .ChangeStatusAsync(brand, request)
                    .ConfigureAwait(false);
            }
        }
    }
}
