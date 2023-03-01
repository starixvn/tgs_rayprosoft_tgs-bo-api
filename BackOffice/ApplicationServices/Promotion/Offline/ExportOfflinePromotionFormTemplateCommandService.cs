using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

internal class ExportOfflinePromotionFormTemplateCommandService
    : IGetService<ExportOfflinePromotionFormTemplateCommand, Stream>
{
    private static readonly EmbeddedFileProvider _EmbeddedFileProvider =
        new(Assembly.GetExecutingAssembly());

    private readonly IFileInfo _templateFileInfo;

    public ExportOfflinePromotionFormTemplateCommandService()
    {
        _templateFileInfo = _EmbeddedFileProvider.GetFileInfo("/Assets/offline_promotion_template.xlsx");
    }

    public ValueTask<Stream> GetAsync(
        ExportOfflinePromotionFormTemplateCommand query,
        CancellationToken cancellationToken = default)
        => ValueTask.FromResult(_templateFileInfo.CreateReadStream());
}
