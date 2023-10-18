using NSubstitute;
using Tgs.ArticleManagement.Clients;
using TGS.Services.BackOffice.ApplicationServices.Articles;

namespace TGS.Tests.BackendOffice.UnitTests;

public class GetDraftFormQueryServiceTests
{
    [Test]
    public async Task 取得草稿內容()
    {
        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        var sut = new GetDraftFormQueryService(fakeArticleManagementClient);

        var query = new GetDraftFormQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = fakeArticleManagementClient.Received(1)
            .GetDraftFormAsync(Arg.Is(query.FormId));
    }
}
