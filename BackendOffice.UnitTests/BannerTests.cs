using NSubstitute;
using NUnit.Framework.Legacy;
using Tgs.Paging;
using TGS.Services.BackOffice.ApplicationServices.Banner;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class BannerTests
{
    [Test]
    public async Task Can_Archive_banner()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new ChangeBannerStatusCommandService(moqBackOfficeRepository);

        var command = new ChangeBannerStatusCommand(
            Guid.NewGuid(),
            4000,
            "User1");

        await sut.ExecuteAsync(command);

        _ = moqBackOfficeRepository.Received(1).ChangeStatusAsync(
            Arg.Is<Guid>(x => x == command.BannerId),
            Arg.Any<int>(),
            Arg.Is<string>(x => x == command.Modifier));
    }

    [Test]
    public async Task User_Can_Change_Banner_DisplayOrder()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new ChangeBannerOrderCommandService(moqBackOfficeRepository);

        var command = new ChangeBannerOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            "User1");

        await sut.ExecuteAsync(command);

        _ = moqBackOfficeRepository.ChangeBannerOrderAsync(
            Arg.Is<Guid>(x => x == command.BannerId),
            Arg.Is<int>(x => x == command.OrderNumber),
            Arg.Is<string>(x => x == command.Modifier));
    }

    [Test]
    public async Task User_Can_Count_Banners_Query()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new GetBannersQueryService(moqBackOfficeRepository);

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

        _ = moqBackOfficeRepository
            .QueryRejectedBannersAsync(Arg.Is<Guid>(x => x == query.BrandId))
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

        ClassicAssert.AreEqual(2, actual);
    }

    [Test]
    public async Task User_Can_Create_Banner_Data()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new CreateBannerCommandService(moqBackOfficeRepository);

        var command = new CreateBannerCommand(
            Guid.NewGuid(),
            "TestTitle",
            DateTimeOffset.Now,
            null,
            null,
            "User1");

        await sut.ExecuteAsync(command);

        _ = moqBackOfficeRepository.Received(1).CreateBannerAsync(
            Arg.Any<BannerModifyInfo>(),
            Arg.Is<string>(x => x == command.Modifier));
    }

    [Test]
    public async Task User_Can_Get_Banner_Detail()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new GetBannerDetailQueryService(moqBackOfficeRepository);

        var query = new GetBannerDetailQuery(
            Guid.NewGuid(),
            Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = moqBackOfficeRepository.Received(1).GetBannerByIdAsync(Arg.Is<Guid>(x => x == query.BannerId));
    }

    [Test]
    public async Task User_Can_Query_Expired_Banners()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new GetBannersQueryService(moqBackOfficeRepository);

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

        _ = moqBackOfficeRepository
            .QueryExpiredBannersAsync(Arg.Is<Guid>(x => x == query.BrandId))
            .Returns(Array.Empty<BannerInfo>().ToAsyncEnumerable());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = moqBackOfficeRepository.Received(1).QueryExpiredBannersAsync(Arg.Is<Guid>(x => x == query.BrandId));
    }

    [Test]
    public async Task User_Can_Query_Rejected_Banners()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new GetBannersQueryService(moqBackOfficeRepository);

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

        _ = moqBackOfficeRepository
            .QueryRejectedBannersAsync(Arg.Is<Guid>(x => x == query.BrandId))
            .Returns(Array.Empty<BannerInfo>().ToAsyncEnumerable());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = moqBackOfficeRepository.Received(1).QueryRejectedBannersAsync(Arg.Is<Guid>(x => x == query.BrandId));
    }

    [Test]
    public async Task User_Can_Query_Reviewing_Banners()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new GetBannersQueryService(moqBackOfficeRepository);

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

        _ = moqBackOfficeRepository
            .QueryReviewingBannersAsync(Arg.Is<Guid>(x => x == query.BrandId))
            .Returns(Array.Empty<BannerInfo>().ToAsyncEnumerable());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = moqBackOfficeRepository.Received(1).QueryReviewingBannersAsync(Arg.Is<Guid>(x => x == query.BrandId));
    }

    [Test]
    public async Task User_Can_Query_Valid_Banners()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new GetBannersQueryService(moqBackOfficeRepository);

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

        _ = moqBackOfficeRepository
            .QueryValidBannersAsync(Arg.Is<Guid>(x => x == query.BrandId))
            .Returns(Array.Empty<BannerInfo>().ToAsyncEnumerable());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = moqBackOfficeRepository.QueryValidBannersAsync(Arg.Is<Guid>(x => x == query.BrandId));
    }

    [Test]
    public async Task User_Can_Review_Banner()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new ReviewBannerCommandService(moqBackOfficeRepository);

        var command = new ReviewBannerCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BannerReviewState.Approve,
            null,
            "User1");

        await sut.ExecuteAsync(command);

        _ = moqBackOfficeRepository.Received(1).ReviewBannerAsync(
            Arg.Is<Guid>(x => x == command.BannerId),
            Arg.Is<BannerReviewState>(x => x == command.ReviewState),
            Arg.Is<string>(x => x == command.Remark),
            Arg.Is<string>(x => x == command.Reviewer));
    }

    [Test]
    public async Task User_Can_Set_Enable_Banner()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new SetIsEanbleBannerCommandService(moqBackOfficeRepository);

        var command = new SetIsEanbleBannerCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            true,
            "User1");

        await sut.ExecuteAsync(command);

        _ = moqBackOfficeRepository.SetIsEnableBannerAsync(
            Arg.Is<Guid>(x => x == command.BannerId),
            Arg.Is<bool>(x => x == command.IsEnable),
            Arg.Is<string>(x => x == command.Modifier));
    }

    [Test]
    public async Task User_Can_Update_Banner_Data()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new UpdateBannerCommandService(moqBackOfficeRepository);

        var command = new UpdateBannerCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TestTitle",
            DateTimeOffset.Now,
            null,
            null,
            "User1");

        await sut.ExecuteAsync(command);

        _ = moqBackOfficeRepository.UpdateBannerAsync(
            Arg.Is<Guid>(x => x == command.BannerId),
            Arg.Any<BannerModifyInfo>(),
            Arg.Is<string>(x => x == command.Modifier));
    }
}