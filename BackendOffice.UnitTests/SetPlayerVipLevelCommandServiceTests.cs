using Microsoft.Extensions.Logging.Abstractions;
using TGS.Services.BackOffice.ApplicationServices.Players;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common.ViewModels;
using Tgs.VipLevelService.Common;

namespace TGS.Tests.BackendOffice.UnitTests;

public class SetPlayerVipLevelCommandServiceTests
{
    [Test]
    public async Task 變更玩家_Vip等級()
    {
        var fakeLogger = NullLogger<SetPlayerVipLevelCommandService>.Instance;
        var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();

        var command = new SetPlayerVipLevelCommand(
            PlayerCode: "4ucmqplxb029",
            ModifyUserId: Guid.Parse("4e84a597-6077-4198-3d04-08d9262ce1a5"),
            ModifyUsername: "Gordon.hung",
            VipId: Guid.Parse("a5e5afe0-e87b-483d-98db-71629ba93290"),
            AutoState: PlayerVipLevelAutoState.Enable,
            Remark: "test");

        var sut = new SetPlayerVipLevelCommandService(fakeLogger, fakeVipLevelServiceClient);

        _ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
            playerCode: Arg.Is(command.PlayerCode),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new PlayerVipLevelInfoViewModel(
                ResellerId: Guid.NewGuid(),
                ResellerCode: "r1",
                LicenseeId: Guid.NewGuid(),
                LicenseeCode: "l1",
                BrandId: Guid.NewGuid(),
                BrandCode: "b1",
                PlayerCode: command.PlayerCode,
                Username: "test1",
                VipLevelId: Guid.Parse("3a6b6f1d-3094-4b66-3c7a-08d9262ce1a5"),
                VipLevel: 1,
                AutoState: PlayerVipLevelAutoState.Enable,
                AccumDeposit: 1000M,
                AccumTurnover: 1000M,
                LevelUpAccumulateBetAmount: 2999m,
                LevelUpAccumulateDeposit: 1999m,
                FullLevel: false,
                NextVipLevelId: Guid.NewGuid(),
                NextVipLevel: 2));

        _ = fakeVipLevelServiceClient.VipLevelGetAsync(
            id: Arg.Is(command.VipId),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new VipLevelInfoViewModel(
                Id: command.VipId,
                Level: 2,
                BrandId: Guid.NewGuid(),
                LevelUpAccumulateBetAmount: 100,
                LevelUpAccumulateDeposit: 100,
                LevelKeepAccumulateBetAmount: 0,
                LevelKeepAccumulateDeposit: 0,
                LevelKeepCondition: true,
                BirthBenefit: 100,
                PromotionBenefit: 100,
                QuarterlyBenefit: 100,
                LiveDealerRebate: 0,
                SlotRebate: 0,
                LotteryRebate: 0,
                CardGameRebate: 0,
                SportsRebate: 0,
                VirtualSportsRebate: 0,
                WithdrawTransactionsPerDay: -1,
                WithdrawTotalAmountPerDay: -1));

        await sut.ExecuteAsync(command);

        _ = fakeVipLevelServiceClient
            .Received(1)
            .PlayerVipLevelManualAsync(
            playerCode: Arg.Is(command.PlayerCode),
            vipLevel: Arg.Is(2),
            autoState: Arg.Is(command.AutoState),
            modifierUserId: Arg.Is(command.ModifyUserId),
            modifierUsername: Arg.Is(command.ModifyUsername),
            remark: Arg.Is(command.Remark));
    }
}
