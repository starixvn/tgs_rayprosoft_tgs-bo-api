using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

internal class GetGameProvidersLocalizationQueryService
    : IQueryService<GetGameProvidersLocalizationQuery, SubGameProviderData>
{
    private readonly ITgsRepository _tgsRepository;

    public GetGameProvidersLocalizationQueryService(ITgsRepository tgsRepository)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
    }

    public IAsyncEnumerable<SubGameProviderData> QueryAsync(
        GetGameProvidersLocalizationQuery query,
        CancellationToken cancellationToken = default)
        => _tgsRepository.SubGameProviders.AsAsyncEnumerable()
            .Select(x => new SubGameProviderData
            {
                SubGameProviderCode = x.Code,
                SubGameProviderName = x.Name,
                Localization = x.Localization
            })
            .ToEnumerable()
            .ToAsyncEnumerable();
}
