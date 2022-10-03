using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Tgs.Paging;
using TGS.Services.BackOffice.Application.Services;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class BannerTests
{
    [Test]
    public async Task User_Can_Change_Banner_DisplayOrder()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new ChangeBannerOrderCommandService(moqBackOfficeRepository.Object);

        var command = new ChangeBannerOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            "User1");

        await sut.ExecuteAsync(command);

        moqBackOfficeRepository.Verify(
            fake => fake.ChangeBannerOrderAsync(
                It.Is<Guid>(x => x == command.BannerId),
                It.Is<int>(x => x == command.OrderNumber),
                It.Is<string>(x => x == command.Modifier)),
            Times.Exactly(1));
    }

    [Test]
    public async Task User_Can_Count_Banners_Query()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new GetBannersQueryService(moqBackOfficeRepository.Object);

        var paging = new Paging<BannerInfo>
        {
            RowCount = 10,
            Page = 0
        };

        paging.SortByPropertyName(nameof(BannerInfo.DisplayOrder), false);

        var query = new GetBannersQuery(
            Guid.NewGuid(),
            BannerStatus.Rejected,
            paging);

        moqBackOfficeRepository
            .Setup(fake => fake.QueryRejectedBannersAsync(It.Is<Guid>(x => x == query.BrandId)))
            .Returns(new[]
            {
                new BannerInfo(
                    Guid.NewGuid(),
                    query.BrandId,
                    "Title1",
                    DateTimeOffset.UtcNow,
                    null,
                    "{}",
                    BannerFlowStatus.Rejected,
                    true,
                    1,
                    "User1",
                    DateTimeOffset.UtcNow),
                new BannerInfo(
                    Guid.NewGuid(),
                    query.BrandId,
                    "Title2",
                    DateTimeOffset.UtcNow,
                    null,
                    "{}",
                    BannerFlowStatus.Rejected,
                    true,
                    1,
                    "User1",
                    DateTimeOffset.UtcNow)
            }.ToAsyncEnumerable());

        var actual = await sut.GetAsync(query);

        Assert.AreEqual(2, actual);
    }

    [Test]
    public async Task User_Can_Create_Banner_Data()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new CreateBannerCommandService(moqBackOfficeRepository.Object);

        var command = new CreateBannerCommand(
            Guid.NewGuid(),
            "TestTitle",
            DateTimeOffset.Now,
            null,
            null,
            "User1");

        await sut.ExecuteAsync(command);

        moqBackOfficeRepository.Verify(
            fake => fake.CreateBannerAsync(
                It.IsAny<BannerModifyInfo>(),
                It.Is<string>(x => x == command.Modifier)),
            Times.Exactly(1));
    }

    [Test]
    public async Task User_Can_Get_Banner_Detail()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new GetBannerDetailQueryService(moqBackOfficeRepository.Object);

        var query = new GetBannerDetailQuery(
            Guid.NewGuid(),
            Guid.NewGuid());

        await sut.GetAsync(query);

        moqBackOfficeRepository.Verify(
            fake => fake.GetBannerByIdAsync(It.Is<Guid>(x => x == query.BannerId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task User_Can_Query_Expired_Banners()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new GetBannersQueryService(moqBackOfficeRepository.Object);

        var paging = new Paging<BannerInfo>
        {
            RowCount = 10,
            Page = 0
        };

        paging.SortByPropertyName(nameof(BannerInfo.DisplayOrder), false);

        var query = new GetBannersQuery(
            Guid.NewGuid(),
            BannerStatus.Expired,
            paging);

        moqBackOfficeRepository
            .Setup(fake => fake.QueryExpiredBannersAsync(It.Is<Guid>(x => x == query.BrandId)))
            .Returns(Array.Empty<BannerInfo>().ToAsyncEnumerable());

        await sut.QueryAsync(query).ToArrayAsync();

        moqBackOfficeRepository.Verify(
            fake => fake.QueryExpiredBannersAsync(It.Is<Guid>(x => x == query.BrandId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task User_Can_Query_Rejected_Banners()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new GetBannersQueryService(moqBackOfficeRepository.Object);

        var paging = new Paging<BannerInfo>
        {
            RowCount = 10,
            Page = 0
        };

        paging.SortByPropertyName(nameof(BannerInfo.DisplayOrder), false);

        var query = new GetBannersQuery(
            Guid.NewGuid(),
            BannerStatus.Rejected,
            paging);

        moqBackOfficeRepository
            .Setup(fake => fake.QueryRejectedBannersAsync(It.Is<Guid>(x => x == query.BrandId)))
            .Returns(Array.Empty<BannerInfo>().ToAsyncEnumerable());

        await sut.QueryAsync(query).ToArrayAsync();

        moqBackOfficeRepository.Verify(
            fake => fake.QueryRejectedBannersAsync(It.Is<Guid>(x => x == query.BrandId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task User_Can_Query_Reviewing_Banners()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new GetBannersQueryService(moqBackOfficeRepository.Object);

        var paging = new Paging<BannerInfo>
        {
            RowCount = 10,
            Page = 0
        };

        paging.SortByPropertyName(nameof(BannerInfo.DisplayOrder), false);

        var query = new GetBannersQuery(
            Guid.NewGuid(),
            BannerStatus.Reviewing,
            paging);

        moqBackOfficeRepository
            .Setup(fake => fake.QueryReviewingBannersAsync(It.Is<Guid>(x => x == query.BrandId)))
            .Returns(Array.Empty<BannerInfo>().ToAsyncEnumerable());

        await sut.QueryAsync(query).ToArrayAsync();

        moqBackOfficeRepository.Verify(
            fake => fake.QueryReviewingBannersAsync(It.Is<Guid>(x => x == query.BrandId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task User_Can_Query_Valid_Banners()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new GetBannersQueryService(moqBackOfficeRepository.Object);

        var paging = new Paging<BannerInfo>
        {
            RowCount = 10,
            Page = 0
        };

        paging.SortByPropertyName(nameof(BannerInfo.DisplayOrder), false);

        var query = new GetBannersQuery(
            Guid.NewGuid(),
            BannerStatus.Valid,
            paging);

        moqBackOfficeRepository
            .Setup(fake => fake.QueryValidBannersAsync(It.Is<Guid>(x => x == query.BrandId)))
            .Returns(Array.Empty<BannerInfo>().ToAsyncEnumerable());

        await sut.QueryAsync(query).ToArrayAsync();

        moqBackOfficeRepository.Verify(
            fake => fake.QueryValidBannersAsync(It.Is<Guid>(x => x == query.BrandId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task User_Can_Review_Banner()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new ReviewBannerCommandService(moqBackOfficeRepository.Object);

        var command = new ReviewBannerCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BannerReviewState.Approve,
            null,
            "User1");

        await sut.ExecuteAsync(command);

        moqBackOfficeRepository.Verify(
            fake => fake.ReviewBannerAsync(
                It.Is<Guid>(x => x == command.BannerId),
                It.Is<BannerReviewState>(x => x == command.ReviewState),
                It.Is<string>(x => x == command.Remark),
                It.Is<string>(x => x == command.Reviewer)),
            Times.Exactly(1));
    }

    [Test]
    public async Task User_Can_Set_Enable_Banner()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new SetIsEanbleBannerCommandService(moqBackOfficeRepository.Object);

        var command = new SetIsEanbleBannerCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            true,
            "User1");

        await sut.ExecuteAsync(command);

        moqBackOfficeRepository.Verify(
            fake => fake.SetIsEnableBannerAsync(
                It.Is<Guid>(x => x == command.BannerId),
                It.Is<bool>(x => x == command.IsEnable),
                It.Is<string>(x => x == command.Modifier)),
            Times.Exactly(1));
    }

    [Test]
    public async Task User_Can_Update_Banner_Data()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new UpdateBannerCommandService(moqBackOfficeRepository.Object);

        var command = new UpdateBannerCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TestTitle",
            DateTimeOffset.Now,
            null,
            null,
            "User1");

        await sut.ExecuteAsync(command);

        moqBackOfficeRepository.Verify(
            fake => fake.UpdateBannerAsync(
                It.Is<Guid>(x => x == command.BannerId),
                It.IsAny<BannerModifyInfo>(),
                It.Is<string>(x => x == command.Modifier)),
            Times.Exactly(1));
    }

    [Test]
    public async Task Can_Archive_banner()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new ChangeBannerStatusCommandService(moqBackOfficeRepository.Object);

        var command = new ChangeBannerStatusCommand(
            Guid.NewGuid(),
            4000,
            "User1");

        await sut.ExecuteAsync(command);

        moqBackOfficeRepository.Verify(
            fake => fake.ChangeStatusAsync(
                It.Is<Guid>(x => x == command.BannerId),
                It.IsAny<int>(),
                It.Is<string>(x => x == command.Modifier)),
            Times.Exactly(1));
    }
}