using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Interfaces;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Application.Commands.BrandLiveChat;

internal class DeleteBrandLiveChatCommandService : ICommandService<DeleteBrandLiveChatCommand>
{
    private readonly ITgsCommands<CoreModels.BrandLiveChat> _commands;
    private readonly ITgsQueries<CoreModels.BrandLiveChat> _tgsQueries;

    public DeleteBrandLiveChatCommandService(
        ITgsQueries<CoreModels.BrandLiveChat> tgsQueries,
        ITgsCommands<CoreModels.BrandLiveChat> commands)
    {
        _tgsQueries = tgsQueries ?? throw new System.ArgumentNullException(nameof(tgsQueries));
        _commands = commands;
    }

    public async ValueTask ExecuteAsync(DeleteBrandLiveChatCommand command, CancellationToken cancellationToken = default)
    {
        var communication = await _tgsQueries.GetSingleAsync(x => x.BrandId == command.BrandId);
        await _commands.DeleteAsync(communication);
    }
}
