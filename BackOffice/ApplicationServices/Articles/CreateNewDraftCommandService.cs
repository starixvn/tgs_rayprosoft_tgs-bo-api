using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ArticleManagement.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Articles;

internal class CreateNewDraftCommandService(IArticleManagementClient articleManagementClient) : IGetService<CreateNewDraftCommand, Guid>
{
    private readonly IArticleManagementClient _articleManagementClient = articleManagementClient ?? throw new ArgumentNullException(nameof(articleManagementClient));

    public ValueTask<Guid> GetAsync(CreateNewDraftCommand command, CancellationToken cancellationToken = default)
    {
        var newForm = new NewDraftForm(
            command.Description,
            command.PublishDate,
            command.LocalizedArticles);

        return _articleManagementClient.CreateNewDraftAsync(command.BrandId, newForm, cancellationToken);
    }
}
