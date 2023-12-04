#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using Tgs.StationLetterService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.StationLetters;

internal class GetLetterByIdQueryService(IStationLetterClient stationLetterClient) : IGetService<GetLetterByIdQuery, StationLetter?>
{
    private readonly IStationLetterClient _stationLetterClient = stationLetterClient ?? throw new ArgumentNullException(nameof(stationLetterClient));

    public ValueTask<StationLetter?> GetAsync(GetLetterByIdQuery query, CancellationToken cancellationToken = default)
        => _stationLetterClient.GetStationLetterAsync(query.LetterId, cancellationToken);
}
