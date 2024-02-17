using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.Agent.Dividend.Client;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Xunit;
using Xunit.Abstractions;
using CreateBrandRequest = TGS.Services.BackOffice.Application.Models.Brand.CreateBrandRequest;
using Enums = TGS.Core.Models.Enums;

namespace BackendOffice.IntegrationTests;

public class BrandTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    private readonly NodeJsFixture _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));

    [Fact]
    public async Task 以BrandId查詢Brand列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var page = 1;
        var rows = 20;
        var sortby = "code";
        var orderby = TGS.Core.Models.Messages.SortOrder.Ascending;
        var showAll = false;

        _ = fakeBrandServiceClient.QueryBrandListAsync(
            Arg.Any<BrandListQuery>(),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandListItem(
                    BrandId: brandId,
                    Status: BrandStatus.Active,
                    null,
                    null,
                    CreatedOn: DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn: DateTime.Parse("2023-01-01 10:00:00"))
            }.ToAsyncEnumerable());

        _ = fakeBrandServiceClient.GetBrandRelationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    brandId,
                    "GPI",
                    "Game Integration",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    "TGS",
                    "TGS")
            }.ToAsyncEnumerable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (request, paging) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandList(request, paging);
            };
        """;

        var request = new BrandListQueryViewModel
        {
            BrandId = brandId
        };

        var paging = new PageableRequest
        {
            OrderBy = orderby,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            ShowAll = showAll
        };

        var expected = new PagedResponse<BrandListItemViewModel>
        {
            TotalCount = 1,
            Valid = true,
            Items = [
                new BrandListItemViewModel
                {
                    Id = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    Code = "GPI",
                    Name = "Game Integration",
                    LicenseeId = Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    LicenseeName = "GPI",
                    ResellerId = Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    ResellerName = "TGS",
                    Status = Enums.BrandStatus.Active,
                    Description = null,
                    OperationalEmail = null,
                    CreatedOn = DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn = DateTime.Parse("2023-01-01 10:00:00")
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { request, paging });

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<BrandListItemViewModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(1, result?.TotalCount);
        expected.Items.ToExpectedObject().ShouldEqual(result?.Items);
    }

    [Fact]
    public async Task 以BrandStatus查詢Brand列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var page = 1;
        var rows = 20;
        var sortby = "code";
        var orderby = TGS.Core.Models.Messages.SortOrder.Ascending;
        var showAll = false;

        _ = fakeBrandServiceClient.QueryBrandListAsync(
            Arg.Any<BrandListQuery>(),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandListItem(
                    BrandId: brandId,
                    Status: BrandStatus.Archived,
                    null,
                    null,
                    CreatedOn: DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn: DateTime.Parse("2023-01-01 10:00:00"))
            }.ToAsyncEnumerable());

        _ = fakeBrandServiceClient.GetBrandRelationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    brandId,
                    "GPI",
                    "Game Integration",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    "TGS",
                    "TGS")
            }.ToAsyncEnumerable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (request, paging) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandList(request, paging);
            };
        """;

        var request = new BrandListQueryViewModel
        {
            Status = Enums.BrandStatus.Archived
        };

        var paging = new PageableRequest
        {
            OrderBy = orderby,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            ShowAll = showAll
        };

        var expected = new PagedResponse<BrandListItemViewModel>
        {
            TotalCount = 1,
            Valid = true,
            Items = [
                new BrandListItemViewModel
                {
                    Id = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    Code = "GPI",
                    Name = "Game Integration",
                    LicenseeId = Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    LicenseeName = "GPI",
                    ResellerId = Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    ResellerName = "TGS",
                    Status = Enums.BrandStatus.Archived,
                    Description = null,
                    OperationalEmail = null,
                    CreatedOn = DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn = DateTime.Parse("2023-01-01 10:00:00")
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { request, paging });

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<BrandListItemViewModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(1, result?.TotalCount);
        expected.Items.ToExpectedObject().ShouldEqual(result?.Items);
    }

    [Fact]
    public async Task 以Keyword查詢Brand列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var page = 1;
        var rows = 20;
        var sortby = "code";
        var orderby = TGS.Core.Models.Messages.SortOrder.Ascending;
        var showAll = false;

        _ = fakeBrandServiceClient.QueryBrandListAsync(
            Arg.Any<BrandListQuery>(),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandListItem(
                    BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    Status: BrandStatus.Active,
                    null,
                    null,
                    CreatedOn: DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn: DateTime.Parse("2023-01-01 10:00:00"))
            }.ToAsyncEnumerable());

        _ = fakeBrandServiceClient.GetBrandRelationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    "GPI",
                    "Game Integration",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    "TGS",
                    "TGS")
            }.ToAsyncEnumerable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (request, paging) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandList(request, paging);
            };
        """;

        var request = new BrandListQueryViewModel
        {
            Keyword = "GPI"
        };

        var paging = new PageableRequest
        {
            OrderBy = orderby,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            ShowAll = showAll
        };

        var expected = new PagedResponse<BrandListItemViewModel>
        {
            TotalCount = 1,
            Valid = true,
            Items = [
                new BrandListItemViewModel
                {
                    Id = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    Code = "GPI",
                    Name = "Game Integration",
                    LicenseeId = Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    LicenseeName = "GPI",
                    ResellerId = Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    ResellerName = "TGS",
                    Status = Enums.BrandStatus.Active,
                    Description = null,
                    OperationalEmail = null,
                    CreatedOn = DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn = DateTime.Parse("2023-01-01 10:00:00")
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { request, paging });

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<BrandListItemViewModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(1, result?.TotalCount);
        expected.Items.ToExpectedObject().ShouldEqual(result?.Items);
    }

    [Fact]
    public async Task 以LicenseeId查詢Brand列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var licenseeId = Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e");

        var page = 1;
        var rows = 20;
        var sortby = "code";
        var orderby = TGS.Core.Models.Messages.SortOrder.Ascending;
        var showAll = false;

        _ = fakeBrandServiceClient.QueryBrandListAsync(
            Arg.Any<BrandListQuery>(),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandListItem(
                    BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    Status: BrandStatus.Active,
                    null,
                    null,
                    CreatedOn: DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn: DateTime.Parse("2023-01-01 10:00:00"))
            }.ToAsyncEnumerable());

        _ = fakeBrandServiceClient.GetBrandRelationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    "GPI",
                    "Game Integration",
                    licenseeId,
                    "GPI",
                    "GPI",
                    Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    "TGS",
                    "TGS")
            }.ToAsyncEnumerable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (request, paging) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandList(request, paging);
            };
        """;

        var request = new BrandListQueryViewModel
        {
            LicenseeId = licenseeId
        };

        var paging = new PageableRequest
        {
            OrderBy = orderby,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            ShowAll = showAll
        };

        var expected = new PagedResponse<BrandListItemViewModel>
        {
            TotalCount = 1,
            Valid = true,
            Items = [
                new BrandListItemViewModel
                {
                    Id = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    Code = "GPI",
                    Name = "Game Integration",
                    LicenseeId = Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    LicenseeName = "GPI",
                    ResellerId = Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    ResellerName = "TGS",
                    Status = Enums.BrandStatus.Active,
                    Description = null,
                    OperationalEmail = null,
                    CreatedOn = DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn = DateTime.Parse("2023-01-01 10:00:00")
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { request, paging });

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<BrandListItemViewModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(1, result?.TotalCount);
        expected.Items.ToExpectedObject().ShouldEqual(result?.Items);
    }

    [Fact]
    public async Task 以ResellerId查詢Brand列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var resellerId = Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460");

        var page = 1;
        var rows = 20;
        var sortby = "code";
        var orderby = TGS.Core.Models.Messages.SortOrder.Ascending;
        var showAll = false;

        _ = fakeBrandServiceClient.QueryBrandListAsync(
            Arg.Any<BrandListQuery>(),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandListItem(
                    BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    Status: BrandStatus.Active,
                    null,
                    null,
                    CreatedOn: DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn: DateTime.Parse("2023-01-01 10:00:00")),
                new BrandListItem(
                    BrandId: Guid.Parse("9f1ecf19-38cd-474f-b0d2-93638dc07d21"),
                    Status: BrandStatus.Active,
                    null,
                    null,
                    CreatedOn: DateTime.Parse("2023-01-10 10:00:00"),
                    UpdatedOn: DateTime.Parse("2023-01-10 10:00:00"))
            }.ToAsyncEnumerable());

        _ = fakeBrandServiceClient.GetBrandRelationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    "GPI",
                    "Game Integration",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    resellerId,
                    "TGS",
                    "TGS"),
                new BrandRelationInfo(
                    Guid.Parse("9f1ecf19-38cd-474f-b0d2-93638dc07d21"),
                    "GPI_HKD",
                    "Game Integration HKD",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    resellerId,
                    "TGS",
                    "TGS")
            }.ToAsyncEnumerable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (request, paging) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandList(request, paging);
            };
        """;

        var request = new BrandListQueryViewModel
        {
            ResellerId = resellerId
        };

        var paging = new PageableRequest
        {
            OrderBy = orderby,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            ShowAll = showAll
        };

        var expected = new PagedResponse<BrandListItemViewModel>
        {
            TotalCount = 2,
            Valid = true,
            Items = [
                new BrandListItemViewModel
                {
                    Id = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    Code = "GPI",
                    Name = "Game Integration",
                    LicenseeId = Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    LicenseeName = "GPI",
                    ResellerId = Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    ResellerName = "TGS",
                    Status = Enums.BrandStatus.Active,
                    Description = null,
                    OperationalEmail = null,
                    CreatedOn = DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn = DateTime.Parse("2023-01-01 10:00:00")
                },
                new BrandListItemViewModel
                {
                    Id = Guid.Parse("9f1ecf19-38cd-474f-b0d2-93638dc07d21"),
                    Code = "GPI_HKD",
                    Name = "Game Integration HKD",
                    LicenseeId = Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    LicenseeName = "GPI",
                    ResellerId = Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    ResellerName = "TGS",
                    Status = Enums.BrandStatus.Active,
                    Description = null,
                    OperationalEmail = null,
                    CreatedOn = DateTime.Parse("2023-01-10 10:00:00"),
                    UpdatedOn = DateTime.Parse("2023-01-10 10:00:00")
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { request, paging });

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<BrandListItemViewModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(2, result?.TotalCount);
        expected.Items.ToExpectedObject().ShouldEqual(result?.Items);
    }

    [Fact]
    public async Task 建立Brand()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeBrandContractClient = Substitute.For<IBrandContractClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeBrandService)
                .AddSingleton(fakeBrandContractClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandRelationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    "GPI",
                    "Game Integration",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    "TGS",
                    "TGS")
            }.ToAsyncEnumerable());

        _ = fakeBrandService.CreateBrandAsync(
                Arg.Any<CreateBrandInfo>(),
                Arg.Any<CancellationToken>())
            .Returns(brandId);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.createBrand(request);
            };
        """;

        var languages = new[] {
            new BrandLanguageModel
            {
                BrandCultureCode = "en-US",
                CultureCode = "en-US"
            } };

        var gameProviders = new[]
        {
            new BrandGameProviderModel
            {
                GameProviderId = Guid.NewGuid(),
                ConfigurationId = Guid.NewGuid(),
                DefaultLanguageCode = "en-US",
                Name = "PPTF"
            }
        };

        var betLimits = new[]
        {
            new BrandBetLimitModel
            {
                Id = 15,
                CurrencyCode = "CNY",
                IsDefault = true
            }
        };

        var request = new TGS.Services.BackOffice.Application.Models.Brand.CreateBrandRequest
        {
            Code = "Brand1",
            Name = "TestBrand",
            Description = "TestBrand",
            OperationalEmail = string.Empty,
            PlatformCharge = 10000m,
            TimeSpanMinutes = 480,
            EnablePlayerIdentity = false,
            LicenseeId = Guid.NewGuid(),
            CurrencyCodes = new[] { "CNY" },
            Languages = languages,
            SettlementIntervalType = Enums.SettlementIntervalType.Daily,
            SettlementDay = 7,
            GameProviders = gameProviders,
            AgentDividendType = Enums.AgentDividendType.OneLayer,
            CheckDividendActiveCount = false,
            AutoClearWageringRequirement = false,
            AutoClearWageringRequirementBalance = 0m,
            DepositHandlingFeeRateTenPercentile = 100m,
            WithdrawHandlingFeeRateTenPercentile = 100m,
            Domains = [],
            BetLimits = betLimits
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { request });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        _ = fakeBrandService.Received(1)
            .CreateBrandAsync(
                Arg.Any<CreateBrandInfo>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 建立Brand時BrandCode不可重複()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeBrandContractClient = Substitute.For<IBrandContractClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeBrandService)
                .AddSingleton(fakeBrandContractClient))
            .Build();

        await web.StartAsync();

        Brand.Culture = CultureInfo.GetCultureInfo("en-US");

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandRelationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    "GPI",
                    "Game Integration",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    "TGS",
                    "TGS")
            }.ToAsyncEnumerable());

        _ = fakeBrandService.CreateBrandAsync(
                Arg.Any<CreateBrandInfo>(),
                Arg.Any<CancellationToken>())
            .Returns(brandId);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.createBrand(request);
            };
        """;

        var languages = new[] {
            new BrandLanguageModel
            {
                BrandCultureCode = "en-US",
                CultureCode = "en-US"
            } };

        var gameProviders = new[]
        {
            new BrandGameProviderModel
            {
                GameProviderId = Guid.NewGuid(),
                ConfigurationId = Guid.NewGuid(),
                DefaultLanguageCode = "en-US",
                Name = "PPTF"
            }
        };

        var betLimits = new[]
        {
            new BrandBetLimitModel
            {
                Id = 15,
                CurrencyCode = "CNY",
                IsDefault = true
            }
        };

        var request = new TGS.Services.BackOffice.Application.Models.Brand.CreateBrandRequest
        {
            Code = "GPI",
            Name = "TestBrand",
            Description = "TestBrand",
            OperationalEmail = string.Empty,
            PlatformCharge = 10000m,
            TimeSpanMinutes = 480,
            EnablePlayerIdentity = false,
            LicenseeId = Guid.NewGuid(),
            CurrencyCodes = new[] { "CNY" },
            Languages = languages,
            SettlementIntervalType = Enums.SettlementIntervalType.Daily,
            SettlementDay = 7,
            GameProviders = gameProviders,
            AgentDividendType = Enums.AgentDividendType.OneLayer,
            CheckDividendActiveCount = false,
            AutoClearWageringRequirement = false,
            AutoClearWageringRequirementBalance = 0m,
            DepositHandlingFeeRateTenPercentile = 100m,
            WithdrawHandlingFeeRateTenPercentile = 100m,
            Domains = [],
            BetLimits = betLimits
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { request });

        // Assert
        Assert.NotNull(actual);
        Assert.False(actual.Valid);
        Assert.Equal(Brand.CodeExisted, actual.Message);

        _ = fakeBrandService.DidNotReceive()
            .CreateBrandAsync(
                Arg.Any<CreateBrandInfo>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 建立Brand時BrandName不可重複()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeBrandContractClient = Substitute.For<IBrandContractClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeBrandService)
                .AddSingleton(fakeBrandContractClient))
            .Build();

        await web.StartAsync();

        Brand.Culture = CultureInfo.GetCultureInfo("en-US");

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandRelationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    "GPI",
                    "Game Integration",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    "TGS",
                    "TGS")
            }.ToAsyncEnumerable());

        _ = fakeBrandService.CreateBrandAsync(
                Arg.Any<CreateBrandInfo>(),
                Arg.Any<CancellationToken>())
            .Returns(brandId);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.createBrand(request);
            };
        """;

        var languages = new[] {
            new BrandLanguageModel
            {
                BrandCultureCode = "en-US",
                CultureCode = "en-US"
            } };

        var gameProviders = new[]
        {
            new BrandGameProviderModel
            {
                GameProviderId = Guid.NewGuid(),
                ConfigurationId = Guid.NewGuid(),
                DefaultLanguageCode = "en-US",
                Name = "PPTF"
            }
        };

        var betLimits = new[]
        {
            new BrandBetLimitModel
            {
                Id = 15,
                CurrencyCode = "CNY",
                IsDefault = true
            }
        };

        var request = new CreateBrandRequest
        {
            Code = "Brand1",
            Name = "Game Integration",
            Description = "TestBrand",
            OperationalEmail = string.Empty,
            PlatformCharge = 10000m,
            TimeSpanMinutes = 480,
            EnablePlayerIdentity = false,
            LicenseeId = Guid.NewGuid(),
            CurrencyCodes = new[] { "CNY" },
            Languages = languages,
            SettlementIntervalType = Enums.SettlementIntervalType.Daily,
            SettlementDay = 7,
            GameProviders = gameProviders,
            AgentDividendType = Enums.AgentDividendType.OneLayer,
            CheckDividendActiveCount = false,
            AutoClearWageringRequirement = false,
            AutoClearWageringRequirementBalance = 0m,
            DepositHandlingFeeRateTenPercentile = 100m,
            WithdrawHandlingFeeRateTenPercentile = 100m,
            Domains = [],
            BetLimits = betLimits
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { request });

        // Assert
        Assert.NotNull(actual);
        Assert.False(actual.Valid);
        Assert.Equal(Brand.NameExisted, actual.Message);

        _ = fakeBrandService.DidNotReceive()
            .CreateBrandAsync(
                Arg.Any<CreateBrandInfo>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 若沒有傳入Status條件Brand列表只會顯示Active與Maintenance的資料()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var page = 1;
        var rows = 20;
        var sortby = "code";
        var orderby = TGS.Core.Models.Messages.SortOrder.Ascending;
        var showAll = false;

        _ = fakeBrandServiceClient.QueryBrandListAsync(
            Arg.Any<BrandListQuery>(),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandListItem(
                    BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    Status: BrandStatus.Active,
                    null,
                    null,
                    CreatedOn: DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn: DateTime.Parse("2023-01-01 10:00:00")),
                new BrandListItem(
                    BrandId: Guid.Parse("9f1ecf19-38cd-474f-b0d2-93638dc07d21"),
                    Status: BrandStatus.Maintenance,
                    null,
                    null,
                    CreatedOn: DateTime.Parse("2023-01-10 10:00:00"),
                    UpdatedOn: DateTime.Parse("2023-01-10 10:00:00")),
                new BrandListItem(
                    BrandId: Guid.Parse("d71ac5ed-b7d9-49f2-a129-8709fa4cc02a"),
                    Status: BrandStatus.Inactive,
                    null,
                    null,
                    CreatedOn: DateTime.Parse("2023-01-02 10:00:00"),
                    UpdatedOn: DateTime.Parse("2023-01-02 10:00:00")),
                new BrandListItem(
                    BrandId: Guid.Parse("c0e1b447-cb16-41b4-b5dc-7935a46c2350"),
                    Status: BrandStatus.Archived,
                    null,
                    null,
                    CreatedOn: DateTime.Parse("2023-01-11 10:00:00"),
                    UpdatedOn: DateTime.Parse("2023-01-11 10:00:00"))
            }.ToAsyncEnumerable());

        _ = fakeBrandServiceClient.GetBrandRelationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    "GPI",
                    "Game Integration",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    "TGS",
                    "TGS"),
                new BrandRelationInfo(
                    Guid.Parse("9f1ecf19-38cd-474f-b0d2-93638dc07d21"),
                    "GPI_HKD",
                    "Game Integration HKD",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    "TGS",
                    "TGS"),
                new BrandRelationInfo(
                    Guid.Parse("d71ac5ed-b7d9-49f2-a129-8709fa4cc02a"),
                    "GPI_Inactive",
                    "Game Integration InActive",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    "TGS",
                    "TGS"),
                new BrandRelationInfo(
                    Guid.Parse("c0e1b447-cb16-41b4-b5dc-7935a46c2350"),
                    "GPI_Archived",
                    "Game Integration Archived",
                    Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    "GPI",
                    "GPI",
                    Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    "TGS",
                    "TGS"),
            }.ToAsyncEnumerable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (request, paging) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandList(request, paging);
            };
        """;

        var request = new BrandListQueryViewModel();

        var paging = new PageableRequest
        {
            OrderBy = orderby,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            ShowAll = showAll
        };

        var expected = new PagedResponse<BrandListItemViewModel>
        {
            TotalCount = 2,
            Valid = true,
            Items = [
                new BrandListItemViewModel
                {
                    Id = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                    Code = "GPI",
                    Name = "Game Integration",
                    LicenseeId = Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    LicenseeName = "GPI",
                    ResellerId = Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    ResellerName = "TGS",
                    Status = Enums.BrandStatus.Active,
                    Description = null,
                    OperationalEmail = null,
                    CreatedOn = DateTime.Parse("2023-01-01 10:00:00"),
                    UpdatedOn = DateTime.Parse("2023-01-01 10:00:00")
                },
                new BrandListItemViewModel
                {
                    Id = Guid.Parse("9f1ecf19-38cd-474f-b0d2-93638dc07d21"),
                    Code = "GPI_HKD",
                    Name = "Game Integration HKD",
                    LicenseeId = Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
                    LicenseeName = "GPI",
                    ResellerId = Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
                    ResellerName = "TGS",
                    Status = Enums.BrandStatus.Maintenance,
                    Description = null,
                    OperationalEmail = null,
                    CreatedOn = DateTime.Parse("2023-01-10 10:00:00"),
                    UpdatedOn = DateTime.Parse("2023-01-10 10:00:00")
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { request, paging });

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<BrandListItemViewModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(2, result?.TotalCount);
        expected.Items.ToExpectedObject().ShouldEqual(result?.Items);
    }

    [Fact]
    public async Task 更新Brand狀態()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI",
                Status = BrandStatus.Active
            });

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId, status) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updateBrandStatus(brandId, status);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { brandId, Enums.BrandStatus.Maintenance.ToString() });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }

    [Fact]
    public async Task 若Brand已經Archived則不會更新Brand狀態()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI",
                Status = BrandStatus.Archived
            });

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId, status) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updateBrandStatus(brandId, status);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { brandId, Enums.BrandStatus.Maintenance.ToString() });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        _ = fakeBrandServiceClient.DidNotReceive()
            .UpdateBrandStatusAsync(
                Arg.Is(brandId),
                Arg.Is(BrandStatus.Maintenance),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 整合測試_取得Brand是否有開啟平台資金告警設定()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI",
                Status = BrandStatus.Archived
            });

        _ = fakeBrandServiceClient.GetBrandPlatformAccountFundAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(true);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandPlatformAccountFundFlag(brandId);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<bool>(
            nodeModule,
            args: new object[] { brandId });

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task 整合測試_設定Brand開啟平台資金告警設定()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var enabled = true;

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI",
                Status = BrandStatus.Archived
            });

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId, enabled) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setBrandPlatformAccountFundFlag(brandId, enabled);
            };
        """;

        // Act
        await nodeService.InvokeFromStringAsync(
             nodeModule,
             args: new object[] { brandId, true });

        // Assert
        _ = fakeBrandServiceClient.Received(1)
            .SetBrandPlatformAccountFundAsync(
                Arg.Is(brandId),
                Arg.Is(enabled),
                Arg.Is(userId.ToString()),
                Arg.Any<CancellationToken>());
    }
}
