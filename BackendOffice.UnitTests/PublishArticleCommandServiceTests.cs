using NSubstitute;
using Tgs.ArticleManagement.Clients;
using TGS.Services.BackOffice.ApplicationServices.Articles;

namespace TGS.Tests.BackendOffice.UnitTests;

public class PublishArticleCommandServiceTests
{
    [Test]
    public async Task 發行文章()
    {
        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        var sut = new PublishArticleCommandService(fakeArticleManagementClient);

        var command = new PublishArticleCommand(Guid.NewGuid(), DateOnly.Parse("2023-10-20"));

        await sut.ExecuteAsync(command);

        _ = fakeArticleManagementClient.Received(1)
            .PublishDraftAsync(
                Arg.Is(command.FormId),
                Arg.Is(command.PublishDate));
    }
}
