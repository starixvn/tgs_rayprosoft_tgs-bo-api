#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using Tgs.StationLetterService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.StationLetters;

internal class GetDeliveryLetterByIdQueryService : IGetService<GetDeliveryLetterByIdQuery, DeliveryLetter?>
{
    private readonly IStationLetterClient _stationLetterClient;

    public GetDeliveryLetterByIdQueryService(IStationLetterClient stationLetterClient)
    {
        _stationLetterClient = stationLetterClient ?? throw new ArgumentNullException(nameof(stationLetterClient));
    }

    public ValueTask<DeliveryLetter?> GetAsync(GetDeliveryLetterByIdQuery query, CancellationToken cancellationToken = default)
        => _stationLetterClient.GetDeliveryLetterAsync(query.DeliveryId, cancellationToken);
}
