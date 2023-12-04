using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Interfaces;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Application.Commands.BrandCommunication;

internal class DeleteBrandCommunicationCommandService(
    ITgsQueries<CoreModels.BrandCommunication> tgsQueries,
    ITgsCommands<CoreModels.BrandCommunication> commands) : ICommandService<DeleteBrandCommunicationCommand>
{
    private readonly ITgsCommands<CoreModels.BrandCommunication> _commands = commands;
    private readonly ITgsQueries<CoreModels.BrandCommunication> _tgsQueries = tgsQueries ?? throw new ArgumentNullException(nameof(tgsQueries));

    public async ValueTask ExecuteAsync(
        DeleteBrandCommunicationCommand command,
        CancellationToken cancellationToken = default)
    {
        var communication = await _tgsQueries.GetSingleAsync(x => x.Id == command.Id);
        await _commands.DeleteAsync(communication);
    }
}
