using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

internal class GetAllSupportLanguagesQueryService : IQueryService<GetAllSupportLanguagesQuery, string>
{
    private readonly ITgsRepository _tgsRepository;

    public GetAllSupportLanguagesQueryService(ITgsRepository tgsRepository)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
    }

    public IAsyncEnumerable<string> QueryAsync(
        GetAllSupportLanguagesQuery query,
        CancellationToken cancellationToken = default)
        => _tgsRepository.GameProviderLanguages.AsAsyncEnumerable()
            .Select(x => x.CultureCode)
            .Distinct()
            .ToEnumerable()
            .ToAsyncEnumerable();
}
