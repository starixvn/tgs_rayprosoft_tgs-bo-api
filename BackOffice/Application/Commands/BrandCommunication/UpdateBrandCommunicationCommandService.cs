using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Interfaces;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Application.Commands.BrandCommunication;

internal class UpdateBrandCommunicationCommandService : ICommandService<UpdateBrandCommunicationCommand>
{
    private readonly ITgsCommands<CoreModels.BrandCommunication> _commands;
    private readonly ITgsQueries<CoreModels.BrandCommunication> _tgsQueries;

    public UpdateBrandCommunicationCommandService(
        ITgsQueries<CoreModels.BrandCommunication> tgsQueries,
        ITgsCommands<CoreModels.BrandCommunication> commands)
    {
        _tgsQueries = tgsQueries ?? throw new ArgumentNullException(nameof(tgsQueries));
        _commands = commands;
    }

    public async ValueTask ExecuteAsync(
        UpdateBrandCommunicationCommand command,
        CancellationToken cancellationToken = default)
    {
        var communication = await _tgsQueries.GetSingleAsync(x => x.Id == command.Id);

        communication.Type = command.CommunicationType;
        communication.Phone = command.Phone;
        communication.AppId = command.AppdId;
        communication.QRCodeUrl = command.QRCodeUrl;

        await _commands.UpdateAsync(communication);
    }
}
