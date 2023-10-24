using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client.JetStream;
using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.Clients;
using TGS.Apis.BackendOffice.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class ArticleManagementTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly NodeJsFixture _nodeJsFixture;

    public ArticleManagementTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));
    }

    [Fact]
    public async Task 列表文章及草稿()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options =>
                    {
                        options.UserId = userId;
                        options.UserName = "system";
                    })
                .Services
                .AddSingleton(fakeArticleManagementClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var rows = new[]
        {
            new DraftListItem(
                Guid.NewGuid(),
                "test1",
                "modifier",
                null,
                ArticleStatus.Draft,
                DateTime.UtcNow,
                DateTime.UtcNow),
            new DraftListItem(
                Guid.NewGuid(),
                "test1",
                "modifier",
                null,
                ArticleStatus.Submission,
                DateTime.UtcNow,
                DateTime.UtcNow)
        };

        var expected = rows.Select(
            item => new DraftListItemViewModel
            {
                Id = item.Id,
                Description = item.Description,
                Modifier = item.Modifier,
                PublishDate = item.PublishDate,
                Status = item.Status,
                CreatedOn = item.CreatedOn,
                UpdatedOn = item.UpdatedOn
            })
            .ToArray();

        _ = fakeArticleManagementClient.ListDraftFormsAsync(
            Arg.Is(brandId),
            Arg.Is(0),
            Arg.Any<CancellationToken>())
            .Returns(new QueryResult<DraftListItem>(
                rows.Length,
                rows.ToAsyncEnumerable()));

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.listDraftForms(brandId);
            };
        """;

        // Act
        var response = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { brandId });

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var actual = JsonSerializer.Deserialize<DraftListItemViewModel[]>(
            response!,
            jsonOptions.JsonSerializerOptions);

        // Assert
        Assert.NotNull(actual);
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 取得文章內容()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options =>
                    {
                        options.UserId = userId;
                        options.UserName = "system";
                    })
                .Services
                .AddSingleton(fakeArticleManagementClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var formId = Guid.NewGuid();

        var draftForm = new DraftForm(
            formId,
            brandId,
            "test1",
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
            "system",
            ArticleStatus.Draft,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow);

        _ = fakeArticleManagementClient.GetDraftFormAsync(
            Arg.Is(formId),
            Arg.Any<CancellationToken>())
            .Returns(draftForm);

        var expected = new DraftFormViewModel
        {
            Id = formId,
            BrandId = brandId,
            CreatedOn = draftForm.CreatedOn,
            Description = draftForm.Description,
            LocalizedArticles = draftForm.LocalizedArticles
                .ToDictionary(
                    kv => kv.Key,
                    kv => new ArticleViewModel
                    {
                        Title = kv.Value.Title,
                        Content = kv.Value.Content,
                        Banners = kv.Value.Banners,
                        Brief = kv.Value.Brief,
                        Writer = kv.Value.Writer
                    })
                .AsReadOnly(),
            Modifier = draftForm.Modifier,
            Status = draftForm.Status,
            UpdatedOn = draftForm.UpdatedOn,
            PublishDate = draftForm.PublishDate
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (formId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getDraftForm(formId);
            };
        """;

        // Act
        var response = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { formId });

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var actual = JsonSerializer.Deserialize<DraftFormViewModel>(
            response!,
            jsonOptions.JsonSerializerOptions);

        // Assert
        Assert.NotNull(actual);
        expected.ToExpectedObject().ShouldMatch(actual);
    }

    [Fact]
    public async Task 建立新草稿()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options =>
                    {
                        options.UserId = userId;
                        options.UserName = "system";
                    })
                .Services
                .AddSingleton(fakeArticleManagementClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var formId = Guid.NewGuid();

        var request = new CreateDraftFormViewModel
        {
            Description = "test1",
            LocalizedArticles = new Dictionary<string, ArticleViewModel>
            {
                ["en-US"] = new ArticleViewModel
                {
                    Title = "title",
                    Brief = "brief",
                    Content = "content",
                    Writer = "writer",
                    Banners = new Dictionary<string, Uri>
                    {
                        ["200"] = new Uri("http://localhost/200.jpg")
                    }.AsReadOnly()
                }
            }.AsReadOnly()
        };

        var expectedNewForm = new NewDraftForm(
                request.Description,
                request.PublishDate,
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
                }.AsReadOnly());

        _ = fakeArticleManagementClient.CreateNewDraftAsync(
            Arg.Is(brandId),
            Arg.Is<NewDraftForm>(form => expectedNewForm.ToExpectedObject(
                ctx => ctx.IgnoreEqualsOverride()).Matches(form)),
            Arg.Any<CancellationToken>())
            .Returns(formId);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.createNewDraftForm(brandId, request);
            };
        """;

        // Act
        var response = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { brandId, request });
        var actual = Guid.Parse(response!);

        // Assert
        Assert.Equal(formId, actual);
    }

    [Fact]
    public async Task 刪除草稿()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options =>
                    {
                        options.UserId = userId;
                        options.UserName = "system";
                    })
                .Services
                .AddSingleton(fakeArticleManagementClient))
            .Build();

        await web.StartAsync();

        var formId = Guid.NewGuid();

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.deleteDraftForm(brandId, request);
            };
        """;

        // Act
        _ = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { formId });

        // Assert
        _ = fakeArticleManagementClient.Received(1)
            .DeleteDraftAsync(Arg.Is(formId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 編輯文章()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options =>
                    {
                        options.UserId = userId;
                        options.UserName = "system";
                    })
                .Services
                .AddSingleton(fakeArticleManagementClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var formId = Guid.NewGuid();

        var request = new ModifyDraftFormViewModel
        {
            Description = "test1",
            LocalizedArticles = new Dictionary<string, ArticleViewModel>
            {
                ["en-US"] = new ArticleViewModel
                {
                    Title = "title",
                    Brief = "brief",
                    Content = "content",
                    Writer = "writer",
                    Banners = new Dictionary<string, Uri>
                    {
                        ["200"] = new Uri("http://localhost/200.jpg")
                    }.AsReadOnly()
                }
            }.AsReadOnly()
        };

        var expectedUpdateForm = new UpdateDraftForm(
                request.Description,
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
                }.AsReadOnly());

        _ = fakeArticleManagementClient.CreateNewDraftAsync(
            Arg.Is(brandId),
            Arg.Is<NewDraftForm>(form => expectedUpdateForm.ToExpectedObject(
                ctx => ctx.IgnoreEqualsOverride()).Matches(form)),
            Arg.Any<CancellationToken>())
            .Returns(formId);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updateDraftForm(brandId, request);
            };
        """;

        // Act
        _ = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { formId, request });

        // Assert
        _ = fakeArticleManagementClient.Received(1)
            .UpdateDraftAsync(
                Arg.Is(formId),
                Arg.Is<UpdateDraftForm>(form => expectedUpdateForm.ToExpectedObject(
                    ctx => ctx.IgnoreEqualsOverride()).Matches(form)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 發行文章()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeArticleManagementClient = Substitute.For<IArticleManagementClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options =>
                    {
                        options.UserId = userId;
                        options.UserName = "system";
                    })
                .Services
                .AddSingleton(fakeArticleManagementClient))
            .Build();

        await web.StartAsync();

        var formId = Guid.NewGuid();
        var publishDate = DateOnly.Parse("2023-10-20");

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, publishDate) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.publishArticle(brandId, publishDate);
            };
        """;

        // Act
        _ = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { formId, publishDate });

        // Assert
        _ = fakeArticleManagementClient.Received(1)
            .PublishDraftAsync(
                Arg.Is(formId),
                Arg.Is(publishDate),
                Arg.Any<CancellationToken>());
    }
}
