#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using Tgs.StationLetterService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.StationLetters;

internal class SendAllPlayersLetterCommandService : ICommandService<SendAllPlayersLetterCommand>
{
    private readonly IStationLetterClient _stationLetterClient;

    public SendAllPlayersLetterCommandService(IStationLetterClient stationLetterClient)
    {
        _stationLetterClient = stationLetterClient ?? throw new ArgumentNullException(nameof(stationLetterClient));
    }

    public ValueTask ExecuteAsync(SendAllPlayersLetterCommand command, CancellationToken cancellationToken = default)
    {
        var request = new BrandAllPlayersStationLetterRequest(
            command.BrandCode,
            command.Subject,
            command.Content,
            command.Operator);

        return _stationLetterClient.SendBrandAllPlayersStationLetterAsync(request, cancellationToken);
    }
}
