using System.Threading.Tasks;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Menus;

internal class SetRoleMenusCommandService : ICommandService<SetRoleMenusCommand>
{
    private readonly IMenuSettingRepository menuSettingRepository;

    public SetRoleMenusCommandService(IMenuSettingRepository menuSettingRepository)
    {
        this.menuSettingRepository = menuSettingRepository;
    }

    public async ValueTask ExecuteAsync(SetRoleMenusCommand command, CancellationToken cancellationToken = default)
    {
        await menuSettingRepository.SetAsync(command.RoleId, command.Menus).ConfigureAwait(false);
    }
}