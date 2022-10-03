using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Services.BatchAdjust;

internal class ExportBatchAdjustFormTemplateCommandService : IGetService<ExportBatchAdjustFormTemplateCommand, Stream>
{
    private static readonly EmbeddedFileProvider _EmbeddedFileProvider =
        new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

    private readonly IFileInfo _templateFileInfo;

    public ExportBatchAdjustFormTemplateCommandService()
    {
        _templateFileInfo = _EmbeddedFileProvider.GetFileInfo("/Assets/batch_adjust_template.xlsx");
    }

    public ValueTask<Stream> GetAsync(ExportBatchAdjustFormTemplateCommand query, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(_templateFileInfo.CreateReadStream());
}
