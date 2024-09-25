using Tgs.ArticleManagement.Clients;
using TGS.Services.BackOffice.ApplicationServices.Articles;

namespace TGS.Tests.BackendOffice.UnitTests;

public class UpdateDraftCommandServiceTests
{
    [Test]
    public async Task 更新草稿()
    {
        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        var sut = new UpdateDraftCommandService(fakeArticleManagementClient);

        var command = new UpdateDraftCommand(Guid.NewGuid(), new UpdateDraftForm(
            "test1",
            new Dictionary<string, Article>()));

        await sut.ExecuteAsync(command);

        _ = fakeArticleManagementClient.Received(1)
            .UpdateDraftAsync(
                Arg.Is(command.FormId),
                Arg.Is(command.UpdateDraftData),
                Arg.Any<CancellationToken>());
    }
}
