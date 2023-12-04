using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ArticleManagement.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Articles;

internal class ListDraftFormsQueryService(IArticleManagementClient articleManagementClient) : IGetService<ListDraftFormsQuery, QueryResult<DraftListItem>>
{
    private readonly IArticleManagementClient _articleManagementClient = articleManagementClient ?? throw new ArgumentNullException(nameof(articleManagementClient));

    public ValueTask<QueryResult<DraftListItem>> GetAsync(ListDraftFormsQuery query, CancellationToken cancellationToken = default)
        => _articleManagementClient.ListDraftFormsAsync(query.BrandId, query.Skip, cancellationToken);
}
