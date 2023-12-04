using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetAllLocalizationStringsQueryService(IBackOfficeRepository backOfficeRepository)
        : IGetService<GetAllLocalizationStringsQuery, IDictionary<string, string>>
{
    private readonly IBackOfficeRepository _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));

    public async ValueTask<IDictionary<string, string>> GetAsync(
        GetAllLocalizationStringsQuery query,
        CancellationToken cancellationToken = default)
        => await _backOfficeRepository
            .QueryAllLocalizationStringsAsync(query.CultureCode, cancellationToken)
            .ToDictionaryAsync(
                p => p.Key,
                p => p.Value,
                cancellationToken)
            .ConfigureAwait(false);
}
