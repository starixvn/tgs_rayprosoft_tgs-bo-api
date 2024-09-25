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

        var query = new ListDraftFormsQuery(Guid.NewGuid(), 123);

        _ = fakeArticleManagementClient.ListDraftFormsAsync(Arg.Is(query.BrandId), Arg.Is(query.Skip))
            .Returns(new QueryResult<DraftListItem>(0, AsyncEnumerable.Empty<DraftListItem>()));

        _ = await sut.GetAsync(query);

        _ = fakeArticleManagementClient.Received(1)
            .ListDraftFormsAsync(Arg.Is(query.BrandId), Arg.Is(query.Skip));
    }
}
