using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ArticleManagement.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Articles;

internal class GetDraftFormQueryService : IGetService<GetDraftFormQuery, DraftForm?>
{
    private readonly IArticleManagementClient _articleManagementClient;

    public GetDraftFormQueryService(IArticleManagementClient articleManagementClient)
    {
        _articleManagementClient = articleManagementClient ?? throw new ArgumentNullException(nameof(articleManagementClient));
    }

    public ValueTask<DraftForm> GetAsync(GetDraftFormQuery query, CancellationToken cancellationToken = default)
        => _articleManagementClient.GetDraftFormAsync(query.FormId, cancellationToken);
}
