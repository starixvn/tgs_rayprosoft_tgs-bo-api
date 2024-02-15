using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Interfaces;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Application.Commands.BrandLiveChat;

internal class UpdateBrandLiveChatCommandService(
    ITgsQueries<CoreModels.BrandLiveChat> tgsQueries,
    ITgsCommands<CoreModels.BrandLiveChat> commands) : ICommandService<UpdateBrandLiveChatCommand>
{
    private readonly ITgsCommands<CoreModels.BrandLiveChat> _commands = commands;
    private readonly ITgsQueries<CoreModels.BrandLiveChat> _tgsQueries = tgsQueries ?? throw new ArgumentNullException(nameof(tgsQueries));

    public async ValueTask ExecuteAsync(UpdateBrandLiveChatCommand command, CancellationToken cancellationToken = default)
    {
        var brandLiveChat = await _tgsQueries.GetSingleAsync(x => x.BrandId == command.BrandId);

        brandLiveChat.ChatName = command.Name;
        brandLiveChat.ChatSetting = JsonSerializer.Serialize(command.Setting);

        await _commands.UpdateAsync(brandLiveChat);
    }
}
