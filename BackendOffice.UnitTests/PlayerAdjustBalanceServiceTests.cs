using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.CashService.Client;
using Tgs.SerialNumberGenerator;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;
using TGS.Services.PlayerBucket;
using TGS.Services.PlayerBucket.Models;
using TGS.Services.WalletService;

namespace TGS.Tests.BackendOffice.UnitTests;

public class PlayerAdjustBalanceServiceTests
{
    [Test]
    public async Task Agent_Adjust_Player_Balance()
    {
        var fakeBackOfficeRepository = Substitute.For<IBackOfficeRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerBucketRepository = Substitute.For<IPlayerBucketRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketRepository,
            fakeSerialNumberGenerator,
            fakeAgentWalletRepository);

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
            "aaa");

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
        var fakePlayerBucketRepository = Substitute.For<IPlayerBucketRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketRepository,
            fakeSerialNumberGenerator,
            fakeAgentWalletRepository);

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
            "aaa");

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
        var fakePlayerBucketRepository = Substitute.For<IPlayerBucketRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketRepository,
            fakeSerialNumberGenerator,
            fakeAgentWalletRepository);

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
            "aaa");

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
        var fakePlayerBucketRepository = Substitute.For<IPlayerBucketRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketRepository,
            fakeSerialNumberGenerator,
            fakeAgentWalletRepository);

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
            "aaa");

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
        var fakePlayerBucketRepository = Substitute.For<IPlayerBucketRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketRepository,
            fakeSerialNumberGenerator,
            fakeAgentWalletRepository);

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
            "aaa");

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

        _ = fakePlayerBucketRepository.Received(1)
            .CreatePlayerBucketsAsync(Arg.Any<CreatePlayerBucketRequest[]>());
        _ = fakeBackOfficeRepository.Received(1)
            .InsertAdjustPlayerRecordAsync(Arg.Is(expectedCreateInfo), Arg.Is(modifyUser));
    }

    [Test]
    public void Transfer_Adjust_Player_Balance()
    {
        var fakeBackOfficeRepository = Substitute.For<IBackOfficeRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerBucketRepository = Substitute.For<IPlayerBucketRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();

        var sut = new PlayerAdjustBalanceService(
            fakeBackOfficeRepository,
            fakeCashServiceClient,
            fakePlayerBucketRepository,
            fakeSerialNumberGenerator,
            fakeAgentWalletRepository);

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
            "aaa");

        var modifyUserId = Guid.NewGuid();
        var modifyUser = "system";

        _ = Assert.ThrowsAsync<NotSupportedException>(
            async () => await sut.ProcessAdjustApplyAsync(applyInfo, modifyUserId, modifyUser));
    }
}
