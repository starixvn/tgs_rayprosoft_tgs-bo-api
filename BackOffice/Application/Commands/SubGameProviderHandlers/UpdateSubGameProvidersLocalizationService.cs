using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Core.Infrastructures.Extensions;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;

namespace TGS.Services.BackOffice.Application.Commands.SubGameProviderHandlers;

internal class
    UpdateSubGameProvidersLocalizationService : IGetService<UpdateSubGameProvidersLocalizationRequest,
        UpdateSubGameProvidersLocationInfo>
{
    private readonly ITgsRepository _tgsRepository;

    public UpdateSubGameProvidersLocalizationService(ITgsRepository tgsRepository)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
    }

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
                game.Localization = model.Localization.ToJson();
                game.UpdatedByUsername = request.EditorUsername;
                game.UpdatedOn = DateTimeOffset.Now;
            }

            await _tgsRepository.SaveChangesAsync(cancellationToken);

            return new UpdateSubGameProvidersLocationInfo { DiffGameProviderCodes = Array.Empty<string>() };
        }

        return new UpdateSubGameProvidersLocationInfo { DiffGameProviderCodes = diffGameProviders };
    }
}
