using NSubstitute;
using Tgs.CashService.Client;
using Tgs.PlayerBucket.Client;
using Tgs.SerialNumberGenerator;
using Tgs.Service.PlayerBuckets;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;
using TGS.Services.WalletService;

namespace TGS.Tests.BackendOffice.UnitTests;

public class PlayerAdjustBalanceServiceTests
{
    [Test]
    public async Task Agent_Adjust_Player_Balance()
    {
        var fakeBackOfficeRepository = Substitute.For<IBackOfficeRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketClient,
            fakeSerialNumberGenerator,
            fakePlayerBucketForTypesClient);

        var applyInfo = new PlayerBalanceAdjustmentApplyInfo(
            Guid.NewGuid(),
            "test1",
            Guid.NewGuid(),
            AdjustmentType.Add,
            PlayerAdjustmentTransactionType.AgentAdjust,
            AdjustTransferType.None,
            100M,
            0M,
            true,
            "aaa",
            BucketTypes: []);

        var modifyUserId = Guid.NewGuid();
        var modifyUser = "system";

        var expectedCreateInfo = new AdjustPlayerRecordCreateInfo(
            applyInfo.BrandId,
            applyInfo.PlayerCode,
            applyInfo.AdjustmentType,
            applyInfo.TransactionType,
            applyInfo.TransferType,
            applyInfo.Amount,
            applyInfo.BucketAmount,
            applyInfo.IncludeInNetProfit,
            applyInfo.Remark,
            Guid.NewGuid());

        _ = fakeCashServiceClient
            .AdjustAgentV2Async(
                Arg.Is(applyInfo.WalletId),
                Arg.Is(applyInfo.Amount),
                Arg.Is(applyInfo.Remark),
                Arg.Is((string?)null),
                Arg.Is(modifyUserId))
            .Returns(new TransactionResponse
            {
                TransactionId = expectedCreateInfo.TransactionId!.Value
            });

        await sut.ProcessAdjustApplyAsync(applyInfo, modifyUserId, modifyUser);

        _ = fakeBackOfficeRepository.Received(1)
            .InsertAdjustPlayerRecordAsync(Arg.Is(expectedCreateInfo), Arg.Is(modifyUser));
    }

    [Test]
    public async Task Bonus_Adjust_Player_Balance()
    {
        var fakeBackOfficeRepository = Substitute.For<IBackOfficeRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketClient,
            fakeSerialNumberGenerator,
            fakePlayerBucketForTypesClient);

        var applyInfo = new PlayerBalanceAdjustmentApplyInfo(
            Guid.NewGuid(),
            "test1",
            Guid.NewGuid(),
            AdjustmentType.Add,
            PlayerAdjustmentTransactionType.BonusAdjust,
            AdjustTransferType.None,
            100M,
            0M,
            true,
            "aaa",
            []);

        var modifyUserId = Guid.NewGuid();
        var modifyUser = "system";

        var expectedCreateInfo = new AdjustPlayerRecordCreateInfo(
            applyInfo.BrandId,
            applyInfo.PlayerCode,
            applyInfo.AdjustmentType,
            applyInfo.TransactionType,
            applyInfo.TransferType,
            applyInfo.Amount,
            applyInfo.BucketAmount,
            applyInfo.IncludeInNetProfit,
            applyInfo.Remark,
            Guid.NewGuid());

        _ = fakeCashServiceClient
            .AdjustBonusV2Async(
                Arg.Is(applyInfo.WalletId),
                Arg.Is(applyInfo.Amount),
                Arg.Is(applyInfo.Remark),
                Arg.Is((string?)null),
                Arg.Is(modifyUserId))
            .Returns(new TransactionResponse
            {
                TransactionId = expectedCreateInfo.TransactionId!.Value
            });

        await sut.ProcessAdjustApplyAsync(applyInfo, modifyUserId, modifyUser);

        _ = fakeBackOfficeRepository.Received(1)
            .InsertAdjustPlayerRecordAsync(Arg.Is(expectedCreateInfo), Arg.Is(modifyUser));
    }

    [Test]
    public async Task Company_Adjust_Player_Balance()
    {
        var fakeBackOfficeRepository = Substitute.For<IBackOfficeRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketClient,
            fakeSerialNumberGenerator,
            fakePlayerBucketForTypesClient);

        var applyInfo = new PlayerBalanceAdjustmentApplyInfo(
            Guid.NewGuid(),
            "test1",
            Guid.NewGuid(),
            AdjustmentType.Add,
            PlayerAdjustmentTransactionType.CompanyAdjust,
            AdjustTransferType.None,
            100M,
            0M,
            true,
            "aaa",
            []);

        var modifyUserId = Guid.NewGuid();
        var modifyUser = "system";

        var expectedCreateInfo = new AdjustPlayerRecordCreateInfo(
            applyInfo.BrandId,
            applyInfo.PlayerCode,
            applyInfo.AdjustmentType,
            applyInfo.TransactionType,
            applyInfo.TransferType,
            applyInfo.Amount,
            applyInfo.BucketAmount,
            applyInfo.IncludeInNetProfit,
            applyInfo.Remark,
            Guid.NewGuid());

        _ = fakeCashServiceClient
            .AdjustCompanyV2Async(
                Arg.Is(applyInfo.WalletId),
                Arg.Is(applyInfo.Amount),
                Arg.Is(applyInfo.Remark),
                Arg.Is((string?)null),
                Arg.Is(modifyUserId))
            .Returns(new TransactionResponse
            {
                TransactionId = expectedCreateInfo.TransactionId!.Value
            });

        await sut.ProcessAdjustApplyAsync(applyInfo, modifyUserId, modifyUser);

        _ = fakeBackOfficeRepository.Received(1)
            .InsertAdjustPlayerRecordAsync(Arg.Is(expectedCreateInfo), Arg.Is(modifyUser));
    }

    [Test]
    public async Task Subtract_Adjust_Player_Balance()
    {
        var fakeBackOfficeRepository = Substitute.For<IBackOfficeRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketClient,
            fakeSerialNumberGenerator,
            fakePlayerBucketForTypesClient);

        var applyInfo = new PlayerBalanceAdjustmentApplyInfo(
            Guid.NewGuid(),
            "test1",
            Guid.NewGuid(),
            AdjustmentType.Subtract,
            PlayerAdjustmentTransactionType.AgentAdjust,
            AdjustTransferType.None,
            100M,
            0M,
            true,
            "aaa",
            []);

        var modifyUserId = Guid.NewGuid();
        var modifyUser = "system";

        var expectedCreateInfo = new AdjustPlayerRecordCreateInfo(
            applyInfo.BrandId,
            applyInfo.PlayerCode,
            applyInfo.AdjustmentType,
            applyInfo.TransactionType,
            applyInfo.TransferType,
            applyInfo.Amount,
            applyInfo.BucketAmount,
            applyInfo.IncludeInNetProfit,
            applyInfo.Remark,
            Guid.NewGuid());

        _ = fakeCashServiceClient
            .AdjustAgentV2Async(
                Arg.Is(applyInfo.WalletId),
                Arg.Is(-applyInfo.Amount),
                Arg.Is(applyInfo.Remark),
                Arg.Is((string?)null),
                Arg.Is(modifyUserId))
            .Returns(new TransactionResponse
            {
                TransactionId = expectedCreateInfo.TransactionId!.Value
            });

        await sut.ProcessAdjustApplyAsync(applyInfo, modifyUserId, modifyUser);

        _ = fakeBackOfficeRepository.Received(1)
            .InsertAdjustPlayerRecordAsync(Arg.Is(expectedCreateInfo), Arg.Is(modifyUser));
    }

    [Test]
    public async Task System_Adjust_Player_Balance()
    {
        var fakeBackOfficeRepository = Substitute.For<IBackOfficeRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketClient,
            fakeSerialNumberGenerator,
            fakePlayerBucketForTypesClient);

        var applyInfo = new PlayerBalanceAdjustmentApplyInfo(
            Guid.NewGuid(),
            "test1",
            Guid.NewGuid(),
            AdjustmentType.Add,
            PlayerAdjustmentTransactionType.SystemAdjust,
            AdjustTransferType.None,
            100M,
            100M,
            true,
            "aaa",
            []);

        var modifyUserId = Guid.NewGuid();
        var modifyUser = "system";

        var expectedCreateInfo = new AdjustPlayerRecordCreateInfo(
            applyInfo.BrandId,
            applyInfo.PlayerCode,
            applyInfo.AdjustmentType,
            applyInfo.TransactionType,
            applyInfo.TransferType,
            applyInfo.Amount,
            applyInfo.BucketAmount,
            applyInfo.IncludeInNetProfit,
            applyInfo.Remark,
            Guid.NewGuid());

        _ = fakeCashServiceClient
            .AdjustSystemV2Async(
                Arg.Is(applyInfo.WalletId),
                Arg.Is(applyInfo.Amount),
                Arg.Is(0M),
                Arg.Is(applyInfo.Remark),
                Arg.Is((string?)null),
                Arg.Is(modifyUserId))
            .Returns(new TransactionResponse
            {
                TransactionId = expectedCreateInfo.TransactionId!.Value
            });

        await sut.ProcessAdjustApplyAsync(applyInfo, modifyUserId, modifyUser);

        _ = fakePlayerBucketForTypesClient.Received(1)
            .CreateBucketForBucketTypesAsync(
                Arg.Is(applyInfo.PlayerCode),
                Arg.Is(applyInfo.BucketAmount),
                Arg.Is(Tgs.Service.PlayerBuckets.BucketType.Adjustment),
                Arg.Any<string>(),
                Arg.Any<string>(),
                bucketTypes: Arg.Is(applyInfo.BucketTypes));
        _ = fakeBackOfficeRepository.Received(1)
            .InsertAdjustPlayerRecordAsync(Arg.Is(expectedCreateInfo), Arg.Is(modifyUser));
    }

    [Test]
    public void Transfer_Adjust_Player_Balance()
    {
        var fakeBackOfficeRepository = Substitute.For<IBackOfficeRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketClient,
            fakeSerialNumberGenerator,
            fakePlayerBucketForTypesClient);

        var applyInfo = new PlayerBalanceAdjustmentApplyInfo(
            Guid.NewGuid(),
            "test1",
            Guid.NewGuid(),
            AdjustmentType.Add,
            PlayerAdjustmentTransactionType.TransferAdjust,
            AdjustTransferType.None,
            100M,
            0M,
            true,
            "aaa",
            []);

        var modifyUserId = Guid.NewGuid();
        var modifyUser = "system";

        _ = Assert.ThrowsAsync<NotSupportedException>(
            async () => await sut.ProcessAdjustApplyAsync(applyInfo, modifyUserId, modifyUser));
    }
}
