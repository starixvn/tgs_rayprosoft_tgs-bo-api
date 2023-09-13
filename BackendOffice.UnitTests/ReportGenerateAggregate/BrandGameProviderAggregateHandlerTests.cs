using NSubstitute;
using StackExchange.Redis;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Redis;
using TGS.Services.BackOffice.Application.Commands.ReportGenerateAggregate;
using TGS.Services.BackOffice.Application.Models.Aggregate;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Valhalla.MessageQueue;

namespace TGS.Tests.BackendOffice.UnitTests.ReportGenerateAggregate;

public class BrandGameProviderAggregateHandlerTests
{
    [Test]
    public async Task Can_Generate_Brand_GameProvider_Aggregate()
    {
        var fakeBrandGameProviderAggregateAuditLogService =
            Substitute.For<IBrandGameProviderAggregateAuditLogService>();
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeRedisCacheManager = Substitute.For<IRedisCacheManager>();

        var sut = new BrandGameProviderAggregateHandler(fakeMessageSender,
            fakeBrandService,
            fakeBrandGameProviderAggregateAuditLogService,
            fakeRedisCacheManager);

        var startDate = DateTimeOffset.Parse("2022/01/01 00:00:00+8");
        var brandId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var request = new GenerateBrandGameProviderAggregateRequest(startDate, brandId)
        {
            UserId = userId
        };

        _ = fakeRedisCacheManager.GetSetAllMembersAsync("aggregate", "brandGP").Returns(Array.Empty<RedisValue>());
        _ = fakeBrandService.GetBrandQueries().Returns(new Brand[]
        {
            new Brand { Id = brandId }
        }.AsQueryable());

        var response = await sut.GetAsync(request, default);

        Assert.IsTrue(response.Valid);

        _ = fakeMessageSender.Received(1)
            .PublishAsync(Arg.Any<string>(), Arg.Any<ReadOnlyMemory<byte>>());
    }

    [Test]
    public async Task Can_Not_Generate_Again_When_The_Previous_Is_Generating()
    {
        var fakeBrandGameProviderAggregateAuditLogService =
            Substitute.For<IBrandGameProviderAggregateAuditLogService>();
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeRedisCacheManager = Substitute.For<IRedisCacheManager>();

        var sut = new BrandGameProviderAggregateHandler(fakeMessageSender,
            fakeBrandService,
            fakeBrandGameProviderAggregateAuditLogService,
            fakeRedisCacheManager);

        var startDate = DateTimeOffset.Parse("2022/01/01 00:00:00+8");
        var brandId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var request = new GenerateBrandGameProviderAggregateRequest(startDate, brandId)
        {
            UserId = userId
        };

        _ = fakeRedisCacheManager.GetSetAllMembersAsync("aggregate", "brandGP").Returns(new RedisValue[]
        { new RedisValue(brandId.ToString())
        });

        _ = fakeBrandService.GetBrandQueries().Returns(new Brand[]
        {
            new Brand { Id = brandId }
        }.AsQueryable());

        var response = await sut.GetAsync(request, default);

        Assert.IsFalse(response.Valid);

        _ = fakeMessageSender.Received(0)
            .PublishAsync(Arg.Any<string>(), Arg.Any<ReadOnlyMemory<byte>>());
    }

    [Test]
    public async Task Can_Not_Generate_Brand_GameProvider_Aggregate_When_Not_Number_One_Of_Month()
    {
        var fakeBrandGameProviderAggregateAuditLogService =
            Substitute.For<IBrandGameProviderAggregateAuditLogService>();
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeRedisCacheManager = Substitute.For<IRedisCacheManager>();

        var sut = new BrandGameProviderAggregateHandler(fakeMessageSender,
            fakeBrandService,
            fakeBrandGameProviderAggregateAuditLogService,
            fakeRedisCacheManager);

        var startDate = DateTimeOffset.Parse("2022/01/02 00:00:00+8");
        var brandId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var request = new GenerateBrandGameProviderAggregateRequest(startDate, brandId)
        {
            UserId = userId
        };

        var response = await sut.GetAsync(request, default);

        Assert.IsFalse(response.Valid);

        _ = fakeMessageSender.Received(0)
            .PublishAsync(Arg.Any<string>(), Arg.Any<ReadOnlyMemory<byte>>());
    }
}
