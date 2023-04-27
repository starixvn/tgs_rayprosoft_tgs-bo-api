#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using Tgs.StationLetterService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.StationLetters;

internal class ReplyLetterCommandService : ICommandService<ReplyLetterCommand>
{
    private readonly IStationLetterClient _stationLetterClient;

    public ReplyLetterCommandService(IStationLetterClient stationLetterClient)
    {
        _stationLetterClient = stationLetterClient ?? throw new ArgumentNullException(nameof(stationLetterClient));
    }

    public ValueTask ExecuteAsync(ReplyLetterCommand command, CancellationToken cancellationToken = default)
    {
        var request = new ReplyStationLetterRequest(
            command.BrandCode,
            command.PlayerCode,
            command.Content);

        return _stationLetterClient.ReplyStationLetterAsync(
            command.LetterId,
            request,
            cancellationToken);
    }
}
