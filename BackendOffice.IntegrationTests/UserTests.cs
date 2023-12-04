using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.Users;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class UserTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    private readonly NodeJsFixture _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));

    [Fact]
    public async Task 查詢管理員列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var resellerId = Guid.NewGuid();
        var licenseeId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var fakeQueries = Substitute.For<ITgsQueries<User>>();

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

                return await svc.getUserList(request, pageableRequest);
            };
        """;

        var roleId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var departmentName = "departmentName";
        var roleName = "roleName";
        var now = DateTime.Now;
        var id = Guid.NewGuid();

        var query = new GetUsersQuery(
            resellerId,
            licenseeId,
            brandId,
            "",
            departmentName,
            roleName,
            "",
            new Tgs.Paging.Paging<UserInfo>
            {
                Page = 1,
                RowCount = 20,
                IsDesc = false
            });

        _ = fakeQueries.GetQueries()
            .Returns(new User[]
            {
                new()
                {
                    Id = id,
                    Username = "username",
                    Name = "name",
                    RoleId = roleId,
                    Role = new Role()
                    {
                        Id = roleId,
                        Name = roleName,
                        ResellerId = resellerId,
                        LicenseeId = licenseeId,
                        BrandId = brandId,
                    },
                    DepartmentId = departmentId,
                    Department = new Department()
                    {
                        Id = departmentId,
                        Name = departmentName
                    },
                    LastLoginIp = "127.0.0.1",
                    LastLoginTime = now,
                    Status = UserStatus.Enabled,
                    CreatedOn = now,
                }
            }.AsQueryable());

        var request = new GetUsersRequest
        {
            Username = "",
            DepartmentName = departmentName,
            Role = roleName
        };

        var pageableRequest = new PageableRequest
        {
            Rows = 20,
            Page = 1,
            SortBy = "Username",
            OrderBy = System.Data.SqlClient.SortOrder.Ascending,
            ShowAll = false
        };

        var expected = new UserModel[]
        {
            new() {
                CreatedOn = new DateTimeOffset(now),
                DepartmentId = departmentId,
                Id = id,
                DepartmentName = departmentName,
                LastLoginIp = "127.0.0.1",
                LastLoginTime = new DateTimeOffset(now),
                LicenseeId = licenseeId,
                BrandId = brandId,
                Name = "name",
                Username = "username",
                RoleId = roleId,
                Role = roleName,
                Status = UserStatus.Enabled.ToString(),
                ResellerId = resellerId,
            }
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { request, pageableRequest });

        // Assert
        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<UserModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.Equal(1, result?.TotalCount);
        expected.ToExpectedObject().ShouldEqual(result?.Items);
    }
}
