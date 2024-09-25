using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;

namespace TGS.Services.BackOffice.Application.Commands.SubGameProviderHandlers;

internal class
    UpdateSubGameProvidersLocalizationService(ITgsRepository tgsRepository) : IGetService<UpdateSubGameProvidersLocalizationRequest,
        UpdateSubGameProvidersLocationInfo>
{
    private readonly ITgsRepository _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));

    public async ValueTask<UpdateSubGameProvidersLocationInfo>
        GetAsync(UpdateSubGameProvidersLocalizationRequest request,
            CancellationToken cancellationToken = default)
    {
        var queryGameProviderCodes = request.SubGameProviders.Select(x => x.Code).ToList();

        var existGameProviderCodes = await _tgsRepository.SubGameProviders.AsQueryable()
            .Where(p => queryGameProviderCodes.Contains(p.Code))
            .Select(p => p.Code)
            .ToArrayAsync(cancellationToken);

        var diffGameProviders =
            request.SubGameProviders.Select(x => x.Code).ToArray().Except(existGameProviderCodes).ToArray();

        if (diffGameProviders.Length == 0)
        {
            foreach (var code in existGameProviderCodes)
            {
                var model = request.SubGameProviders.First(x => x.Code == code);
                var game = await _tgsRepository.SubGameProviders.AsQueryable().FirstOrDefaultAsync(
                    x => x.Code == code,
                    cancellationToken);
                game.Localization = JsonSerializer.Serialize(model.Localization);
                game.UpdatedByUsername = request.EditorUsername;
                game.UpdatedOn = DateTimeOffset.Now;
            }

            _ = await _tgsRepository.SaveChangesAsync(cancellationToken);

            return new UpdateSubGameProvidersLocationInfo
            {
                DiffGameProviderCodes = []
            };
        }

        return new UpdateSubGameProvidersLocationInfo { DiffGameProviderCodes = diffGameProviders };
    }
}
