using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.Infrastructures.Extensions;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Application.Commands.BrandLiveChat;

internal class CreateBrandLiveChatCommandService(ITgsCommands<CoreModels.BrandLiveChat> commands) : ICommandService<CreateBrandLiveChatCommand>
{
    private readonly ITgsCommands<CoreModels.BrandLiveChat> _commands = commands;

    public async ValueTask ExecuteAsync(CreateBrandLiveChatCommand command, CancellationToken cancellationToken = default)
    {
        var brandLiveChat = new CoreModels.BrandLiveChat()
        {
            BrandId = command.BrandId,
            ChatName = command.Name,
            ChatSetting = command.Setting.ToJson()
        };
        await _commands.CreateAsync(brandLiveChat);
    }
}
