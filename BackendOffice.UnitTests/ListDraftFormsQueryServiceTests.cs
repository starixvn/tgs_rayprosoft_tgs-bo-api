using NSubstitute;
using Tgs.ArticleManagement.Clients;
using TGS.Services.BackOffice.ApplicationServices.Articles;

namespace TGS.Tests.BackendOffice.UnitTests;

public class ListDraftFormsQueryServiceTests
{
    [Test]
    public async Task 呼叫Api取得文章草稿列表()
    {
        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        var sut = new ListDraftFormsQueryService(fakeArticleManagementClient);

        var query = new ListDraftFormsQuery(Guid.NewGuid());

        _ = fakeArticleManagementClient.ListDraftFormsAsync(Arg.Is(query.BrandId), Arg.Is(0))
            .Returns(new QueryResult<DraftListItem>(0, AsyncEnumerable.Empty<DraftListItem>()));

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeArticleManagementClient.Received(1)
            .ListDraftFormsAsync(Arg.Is(query.BrandId), Arg.Is(0));
    }
}
