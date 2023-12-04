using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ArticleManagement.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Articles;

internal class UpdateDraftCommandService(IArticleManagementClient articleManagementClient) : ICommandService<UpdateDraftCommand>
{
    private readonly IArticleManagementClient _articleManagementClient = articleManagementClient ?? throw new ArgumentNullException(nameof(articleManagementClient));

    public ValueTask ExecuteAsync(UpdateDraftCommand command, CancellationToken cancellationToken = default)
    {
        var updateData = new UpdateDraftForm(
            command.UpdateDraftData.Description,
            command.UpdateDraftData.LocalizedArticles);

        return _articleManagementClient.UpdateDraftAsync(command.FormId, updateData, cancellationToken);
    }
}
