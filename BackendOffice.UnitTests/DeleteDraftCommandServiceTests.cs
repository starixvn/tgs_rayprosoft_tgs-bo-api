using Tgs.ArticleManagement.Clients;
using TGS.Services.BackOffice.ApplicationServices.Articles;

namespace TGS.Tests.BackendOffice.UnitTests;

public class DeleteDraftCommandServiceTests
{
    [Test]
    public async Task 刪除草稿()
    {
        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        var sut = new DeleteDraftCommandService(fakeArticleManagementClient);

        var command = new DeleteDraftCommand(Guid.NewGuid());

        await sut.ExecuteAsync(command);

        _ = fakeArticleManagementClient.Received(1)
            .DeleteDraftAsync(Arg.Is(command.FormId));
    }
}
