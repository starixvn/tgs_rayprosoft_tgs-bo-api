using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.Infrastructures.Extensions;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Application.Commands.BrandLiveChat;

internal class UpdateBrandLiveChatCommandService : ICommandService<UpdateBrandLiveChatCommand>
{
    private readonly ITgsCommands<CoreModels.BrandLiveChat> _commands;
    private readonly ITgsQueries<CoreModels.BrandLiveChat> _tgsQueries;

    public UpdateBrandLiveChatCommandService(
        ITgsQueries<CoreModels.BrandLiveChat> tgsQueries,
        ITgsCommands<CoreModels.BrandLiveChat> commands)
    {
        _tgsQueries = tgsQueries ?? throw new ArgumentNullException(nameof(tgsQueries));
        _commands = commands;
    }

    public async ValueTask ExecuteAsync(UpdateBrandLiveChatCommand command, CancellationToken cancellationToken = default)
    {
        var brandLiveChat = await _tgsQueries.GetSingleAsync(x => x.BrandId == command.BrandId);

        brandLiveChat.ChatName = command.Name;
        brandLiveChat.ChatSetting = command.Setting.ToJson();

        await _commands.UpdateAsync(brandLiveChat);
    }
}
