using System;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using TGS.Services.BackOffice.Application.Models.PlayerBucket;
using TGS.Services.BackOffice.Application.Queries.PlayerBucketQueries;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class PlayerBucketsTests
{
    [Test]
    public async Task Can_Get_Player_Buckets()
    {
        var fakePlayerBucketService = Substitute.For<IPlayerBucketService>();
        var sut = new GetPlayerBucketsQuery(fakePlayerBucketService);

        var playerBuckets = new Core.DataServices.Tgs.Models.PlayerBucket[]
        {
            new Core.DataServices.Tgs.Models.PlayerBucket
            {
                PlayerCode="playerCode",
                IsFull = false
            } ,
            new Core.DataServices.Tgs.Models.PlayerBucket
            {
                PlayerCode="playerCode",
                IsFull = true
            }
        };

        fakePlayerBucketService.GetPlayerBucketQueries().Returns(playerBuckets.AsQueryable().BuildMock());

        var actual = await sut.GetAsync(new GetPlayerBucketsRequest("playerCode"));

        Assert.AreEqual(1, actual.Items.Length);
    }

    [Test]
    public async Task Get_Player_Buckets_Order_Descending_By_Created_On()
    {
        var fakePlayerBucketService = Substitute.For<IPlayerBucketService>();
        var sut = new GetPlayerBucketsQuery(fakePlayerBucketService);

        var playerBuckets = new Core.DataServices.Tgs.Models.PlayerBucket[]
        {
            new Core.DataServices.Tgs.Models.PlayerBucket
            {
                PlayerCode="playerCode",
                IsFull = false,
                CreatedOn = new DateTimeOffset(2022,1,7,0,31,11,TimeSpan.Zero)
            } ,
            new Core.DataServices.Tgs.Models.PlayerBucket
            {
                PlayerCode="playerCode",
                IsFull = false,
                CreatedOn = new DateTimeOffset(2022,3,17,0,31,13,TimeSpan.Zero)
            }
        };

        fakePlayerBucketService.GetPlayerBucketQueries().Returns(playerBuckets.AsQueryable().BuildMock());

        var actual = await sut.GetAsync(new GetPlayerBucketsRequest("playerCode"));

        Assert.AreEqual(
            new DateTimeOffset(2022, 3, 17, 0, 31, 13, TimeSpan.Zero),
            actual.Items.First().CreatedOn);
    }
}
