using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ArticleManagement.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Articles;

internal class ListDraftFormsQueryService : IQueryService<ListDraftFormsQuery, DraftListItem>
{
    private readonly IArticleManagementClient _articleManagementClient;

    public ListDraftFormsQueryService(IArticleManagementClient articleManagementClient)
    {
        _articleManagementClient = articleManagementClient ?? throw new ArgumentNullException(nameof(articleManagementClient));
    }

    public IAsyncEnumerable<DraftListItem> QueryAsync(ListDraftFormsQuery query, CancellationToken cancellationToken = default)
        => _articleManagementClient.ListDraftFormsAsync(query.BrandId, cancellationToken);
}
