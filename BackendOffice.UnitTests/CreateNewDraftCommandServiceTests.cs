using NSubstitute;
using Tgs.ArticleManagement.Clients;
using TGS.Services.BackOffice.ApplicationServices.Articles;

namespace TGS.Tests.BackendOffice.UnitTests;

public class CreateNewDraftCommandServiceTests
{
    [Test]
    public async Task 建立草稿()
    {
        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        var sut = new CreateNewDraftCommandService(fakeArticleManagementClient);

        var command = new CreateNewDraftCommand(
            Guid.NewGuid(),
            "test1",
            null,
            new Dictionary<string, Article>
            {
                ["en-US"] = new Article(
                    "title",
                    "brief",
                    "content",
                    "writer",
                    new Dictionary<string, Uri>
                    {
                        ["200"] = new Uri("http://localhost/200.jpg")
                    }.AsReadOnly())
            }.AsReadOnly(),
            "modifier");

        _ = await sut.GetAsync(command);

        _ = fakeArticleManagementClient.Received(1)
            .CreateNewDraftAsync(
                Arg.Is(command.BrandId),
                Arg.Is(new NewDraftForm(
                    command.Description,
                    command.PublishDate,
                    command.LocalizedArticles)));
    }
}
