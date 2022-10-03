using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NUnit.Framework;
using TGS.BackOffice.Repositories;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class BackOfficeRepositoryTests
{
    [Test]
    public async Task Query_Banners_By_Expired()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        var dbContext = new TgsRepository(dbContextOptionsBuilder.Options);

        var moqSystemClock = new Mock<ISystemClock>();

        var sut = new BackOfficeRepository(dbContext, moqSystemClock.Object);

        var brandId = Guid.NewGuid();
        var currentTime = new DateTimeOffset(
            2021,
            8,
            19,
            0,
            0,
            0,
            TimeSpan.FromHours(8));

        moqSystemClock
            .Setup(fake => fake.UtcNow)
            .Returns(currentTime);

        await dbContext.Banners.AddRangeAsync(
            new[] {
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 1000,
                    EndTime = null,
                    Title = "Title1",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                },
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 1000,
                    EndTime = currentTime.AddDays(1),
                    Title = "Title2",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                },
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 1000,
                    EndTime = currentTime.AddDays(-1),
                    Title = "Title3",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                },
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 2000,
                    EndTime = currentTime.AddDays(1),
                    Title = "Title4",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                }
            });

        await dbContext.SaveChangesAsync();

        var actual = await sut.QueryExpiredBannersAsync(brandId).CountAsync();

        Assert.AreEqual(1, actual);
    }

    [Test]
    public async Task Query_Banners_By_Rejected()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        var dbContext = new TgsRepository(dbContextOptionsBuilder.Options);

        var moqSystemClock = new Mock<ISystemClock>();

        var sut = new BackOfficeRepository(dbContext, moqSystemClock.Object);

        var brandId = Guid.NewGuid();
        var currentTime = new DateTimeOffset(
            2021,
            8,
            19,
            0,
            0,
            0,
            TimeSpan.FromHours(8));

        moqSystemClock
            .Setup(fake => fake.UtcNow)
            .Returns(currentTime);

        await dbContext.Banners.AddRangeAsync(
            new[] {
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 1000,
                    EndTime = null,
                    Title = "Title1",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                },
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 2000,
                    EndTime = currentTime.AddDays(1),
                    Title = "Title2",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                }
            });

        await dbContext.SaveChangesAsync();

        var actual = await sut.QueryRejectedBannersAsync(brandId).CountAsync();

        Assert.AreEqual(1, actual);
    }

    [Test]
    public async Task Query_Banners_By_Reviewing()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        var dbContext = new TgsRepository(dbContextOptionsBuilder.Options);

        var sut = new BackOfficeRepository(dbContext, new DefaultSystemClock());

        var brandId = Guid.NewGuid();

        await dbContext.Banners.AddRangeAsync(
            new[] {
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 0,
                    Title = "Title1",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                },
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 1000,
                    Title = "Title2",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                }
            });

        await dbContext.SaveChangesAsync();

        var actual = await sut.QueryReviewingBannersAsync(brandId).CountAsync();

        Assert.AreEqual(1, actual);
    }

    [Test]
    public async Task Query_Banners_By_Valid()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        var dbContext = new TgsRepository(dbContextOptionsBuilder.Options);

        var moqSystemClock = new Mock<ISystemClock>();

        var sut = new BackOfficeRepository(dbContext, moqSystemClock.Object);

        var brandId = Guid.NewGuid();
        var currentTime = new DateTimeOffset(
            2021,
            8,
            19,
            0,
            0,
            0,
            TimeSpan.FromHours(8));

        moqSystemClock
            .Setup(fake => fake.UtcNow)
            .Returns(currentTime);

        await dbContext.Banners.AddRangeAsync(
            new[] {
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 1000,
                    EndTime = null,
                    Title = "Title1",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                },
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 1000,
                    EndTime = currentTime.AddDays(1),
                    Title = "Title2",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                },
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 1000,
                    EndTime = currentTime.AddDays(-1),
                    Title = "Title3",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                },
                new Banner
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Status = 2000,
                    EndTime = currentTime.AddDays(1),
                    Title = "Title4",
                    CreatedByUserName = "System",
                    UpdatedByUserName = "System"
                }
            });

        await dbContext.SaveChangesAsync();

        var actual = await sut.QueryValidBannersAsync(brandId).CountAsync();

        Assert.AreEqual(2, actual);
    }
}
