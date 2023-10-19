using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ArticleManagement.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Articles;

internal class DeleteDraftCommandService : ICommandService<DeleteDraftCommand>
{
    private readonly IArticleManagementClient _articleManagementClient;

    public DeleteDraftCommandService(IArticleManagementClient articleManagementClient)
    {
        _articleManagementClient = articleManagementClient ?? throw new ArgumentNullException(nameof(articleManagementClient));
    }

    public ValueTask ExecuteAsync(DeleteDraftCommand command, CancellationToken cancellationToken = default)
        => _articleManagementClient.DeleteDraftAsync(command.FormId, cancellationToken);
}
