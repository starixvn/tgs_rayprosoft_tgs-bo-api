using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ArticleManagement.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Articles;

internal class PublishArticleCommandService : ICommandService<PublishArticleCommand>
{
    private readonly IArticleManagementClient _articleManagementClient;

    public PublishArticleCommandService(IArticleManagementClient articleManagementClient)
    {
        _articleManagementClient = articleManagementClient ?? throw new ArgumentNullException(nameof(articleManagementClient));
    }

    public ValueTask ExecuteAsync(PublishArticleCommand command, CancellationToken cancellationToken = default)
        => _articleManagementClient.PublishDraftAsync(
            command.FormId,
            command.PublishDate,
            cancellationToken);
}
