using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class ExportOfflineLoginRewardFormTemplateCommandService 
    : IGetService<ExportOfflineLoginRewardFormTemplateCommand, Stream>
{
    private static readonly EmbeddedFileProvider _EmbeddedFileProvider =
        new(Assembly.GetExecutingAssembly());

    private readonly IFileInfo _templateFileInfo;

    public ExportOfflineLoginRewardFormTemplateCommandService()
    {
        _templateFileInfo = _EmbeddedFileProvider.GetFileInfo("/Assets/offline_login_reward_template.xlsx");
    }

    public ValueTask<Stream> GetAsync(
        ExportOfflineLoginRewardFormTemplateCommand query,
        CancellationToken cancellationToken = default) 
        => ValueTask.FromResult(_templateFileInfo.CreateReadStream());
}