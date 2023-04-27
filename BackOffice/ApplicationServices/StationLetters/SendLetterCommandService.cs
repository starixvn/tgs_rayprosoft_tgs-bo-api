using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using Tgs.StationLetterService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.StationLetters;

internal class SendLetterCommandService : ICommandService<SendLetterCommand>
{
    private readonly IStationLetterClient _stationLetterClient;

    public SendLetterCommandService(IStationLetterClient stationLetterClient)
    {
        _stationLetterClient = stationLetterClient ?? throw new ArgumentNullException(nameof(stationLetterClient));
    }

    public ValueTask ExecuteAsync(SendLetterCommand command, CancellationToken cancellationToken = default)
    {
        var request = new StationLetterRequest(
            command.BrandCode,
            command.PlayerCodes,
            command.Subject,
            command.Content);

        return _stationLetterClient.SendStationLetterAsync(request, cancellationToken);
    }
}
