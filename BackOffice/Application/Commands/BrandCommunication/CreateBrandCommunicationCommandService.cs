using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.Infrastructures.Extensions;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Application.Commands.BrandCommunication;

internal class CreateBrandCommunicationCommandService(
    ITgsCommands<CoreModels.BrandCommunication> commands) : ICommandService<CreateBrandCommunicationCommand>
{
    private readonly ITgsCommands<CoreModels.BrandCommunication> _commands = commands;

    public async ValueTask ExecuteAsync(
        CreateBrandCommunicationCommand command,
        CancellationToken cancellationToken = default)
    {
        var brandCommunication = new CoreModels.BrandCommunication()
        {
            Id = GuidExtensions.NewSequentialGuid(),
            BrandId = command.BrandId,
            Type = command.CommunicationType,
            Phone = command.Phone,
            AppId = command.AppId,
            QRCodeUrl = command.QRCodeUrl
        };

        await _commands.CreateAsync(brandCommunication);
    }
}
