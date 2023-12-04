using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using TGS.Apis.BackendOffice.Controllers.Messages.Department;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class DepartmentTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    private readonly NodeJsFixture _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));

    [Fact]
    public async Task 查詢部門列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var resellerId = Guid.NewGuid();
        var licenseeId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var fakeQueries = Substitute.For<ITgsQueries<Department>>();

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
                .AddSingleton(fakeQueries)
                )
            .Build();

        await web.StartAsync();

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (request, pageableRequest) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getDepartmentList(request, pageableRequest);
            };
        """;

        var roleId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var departmentName = "DepartmentName";
        var now = DateTime.Now;
        var id = Guid.NewGuid();

        var query = new GetDepartmentsQuery(
            resellerId,
            licenseeId,
            brandId,
            departmentName,
            new Tgs.Paging.Paging<DepartmentInfo>
            {
                Page = 1,
                RowCount = 20,
                IsDesc = false
            });

        _ = fakeQueries
            .GetQueries()
            .Returns(new Department[]
            {
                new()
                {
                    Id = departmentId,
                    Name = departmentName,
                    ResellerId = resellerId,
                    Reseller = new Reseller()
                    {
                        Id = resellerId,
                        Name = "ResellerName"
                    },
                    LicenseeId = licenseeId,
                    Licensee = new Licensee()
                    {
                        Id = licenseeId,
                        Name = "LicenseeName"
                    },
                    BrandId = brandId,
                    Brand = new Brand
                    {
                        Id = brandId,
                        Name = "BrandName"
                    },
                    CreatedOn = DateTime.Now.AddMonths(-1),
                }
            }.AsQueryable());

        var request = new GetDepartmentsRequest
        {
            Name = departmentName
        };

        var pageableRequest = new PageableRequest
        {
            Rows = 20,
            Page = 1,
            SortBy = "Name",
            OrderBy = System.Data.SqlClient.SortOrder.Ascending,
            ShowAll = false
        };

        var expected = new DepartmentModel[]
        {
            new() {
                Id = departmentId,
                Name = departmentName,
                LicenseeId = licenseeId,
                BrandId = brandId,
                ResellerId = resellerId,
                ResellerName = "ResellerName",
                LicenseeName = "LicenseeName",
                BrandName = "BrandName"
            }
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { request, pageableRequest });

        // Assert
        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<DepartmentModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.Equal(1, result?.TotalCount);
        expected.ToExpectedObject().ShouldEqual(result?.Items);
    }
}
