#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using Tgs.StationLetterService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.StationLetters;

internal class GetBrandLettersQueryService(IStationLetterClient stationLetterClient) : IQueryService<GetBrandLettersQuery, StationLetter>
{
    private readonly IStationLetterClient _stationLetterClient = stationLetterClient ?? throw new ArgumentNullException(nameof(stationLetterClient));

    public IAsyncEnumerable<StationLetter> QueryAsync(GetBrandLettersQuery query, CancellationToken cancellationToken = default)
        => _stationLetterClient.ListStationLettersAsync(query.BrandCode, cancellationToken);
}
