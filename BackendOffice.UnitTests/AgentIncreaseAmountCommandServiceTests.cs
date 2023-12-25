using NSubstitute;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.CashService.Client;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentIncreaseAmountCommandServiceTests
{
    [Test]
    public async Task 玩家錢包加錢()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();

        var sut = new AgentIncreaseAmountCommandService(
            fakeAgentClient,
            fakeCashServiceClient,
            fakeAdjustAgentAccountService);

        var command = new AgentIncreaseAmountCommand(
            Guid.NewGuid(),
            1000M,
            "test",
            AgentAdjustmentTransactionType.Dividend,
            Guid.NewGuid(),
            "test1");
        var walletId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Is(new GetAgentInfoRequest(command.AgentId)))
            .Returns(new AgentInfoModel(
                command.AgentId,
                "agent1",
                "agent1",
                null,
                null,
                null,
                null,
                "abcd",
                null,
                DateTimeOffset.UtcNow,
                Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Agent,
                0M,
                0,
                0M,
                brandId,
                Guid.NewGuid(),
                walletId,
                false));

        _ = fakeCashServiceClient.AdjustCompanyV2Async(
            Arg.Is(walletId),
            Arg.Is(command.Amount),
            Arg.Is(command.Remark),
            Arg.Any<string>(),
            Arg.Is(command.UserId))
            .Returns(new TransactionResponse
            {
                Balance = 0M,
                LockedBalance = 0,
                TransactionId = transactionId
            });

        _ = await sut.GetAsync(command);

        _ = fakeCashServiceClient.Received(1)
            .AdjustCompanyV2Async(
                Arg.Is(walletId),
                Arg.Is(command.Amount),
                Arg.Is(command.Remark),
                Arg.Any<string>(),
                Arg.Is(command.UserId));
        _ = fakeAdjustAgentAccountService.Received(1)
            .CreateAsync(
                Arg.Is(transactionId),
                Arg.Is(brandId),
                Arg.Is(command.AgentId),
                Arg.Is(AdjustmentType.Add),
                Arg.Is(command.TransactionType),
                Arg.Is(AdjustTransferType.None),
                Arg.Is(command.Amount),
                Arg.Is(command.Remark),
                Arg.Is(command.UserName));
    }
}
