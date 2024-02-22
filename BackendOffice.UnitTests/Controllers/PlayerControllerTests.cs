using System.Collections.Immutable;
using AutoFixture;
using ExpectedObjects;
using FluentAssertions;
using MediatR;
using MockQueryable.Moq;
using Tgs.CashService.Client;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common.ViewModels;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.ApplicationServices.Players;
using TGS.Services.BackOffice.Domain;
using PlayerInfo = Tgs.PlayerService.Client.PlayerInfo;
using PlayerStatus = TGS.Core.Models.Enums.PlayerStatus;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class PlayerControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Get_Player()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeServiceCoordinator = Substitute.For<IServiceCoordinator>();
        var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();

        var sut = new GetSinglePlayerQueryService(
            fakePlayerServiceClient,
            fakePaymentServiceClient,
            fakeCashServiceClient,
            fakePlayerBucketClient,
            fakeServiceCoordinator,
            fakeVipLevelServiceClient);

        var query = new GetSinglePlayerQuery("test1");

        var agentId = Guid.NewGuid();
        var defaultWalletId = Guid.NewGuid();

        var expected = new
        {
            PlayerCode = "test1",
            AgentUsername = "agent1",
            AreaCode = "333",
            Phone = "456789",
            Balance = 500M,
            Birthdate = "2022/11/09",
            Email = "test@test.com",
            IdentityAuthentication = false,
            Properties = new Dictionary<string, string>
            {
                { "DefaultWalletId", defaultWalletId.ToString() },
                { "AgentId" , agentId.ToString() },
                { "AreaCode" , "333" },
                { "Phone" , "456789" },
                { "Birthdate" , "2022/11/09" },
                { "Email" , "test@test.com" },
                { "Zalo", "testZalo" }
            },
            VipId = Guid.NewGuid(),
            VipLevel = 1,
            VipLevelAutoState = true,
            AccumDeposit = 500m,
            AccumTurnover = 0.0m,
            LevelUpAccumulateBetAmount = 1999m,
            LevelUpAccumulateDeposit = 999m,
            FullLevel = false,
            NextVipLevelId = Guid.NewGuid(),
            NextVipLevel = 2,
        };

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(
                Arg.Is(query.PlayerCode))
            .Returns(new PlayerInfo
            {
                PlayerCode = query.PlayerCode,
                BrandId = Guid.NewGuid(),
                UserName = "tester",
                RegisteredOn = DateTime.UtcNow,
                IdentityAuthentication = false
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(query.PlayerCode))
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = defaultWalletId.ToString(),
                ["AgentId"] = agentId.ToString(),
                ["AreaCode"] = expected.AreaCode,
                ["Phone"] = expected.Phone,
                ["Birthdate"] = expected.Birthdate,
                ["Email"] = expected.Email,
                ["Zalo"] = "testZalo",
            }.ToImmutableDictionary());

        _ = fakePlayerBucketClient.ReadBucketsAsync(Arg.Is(query.PlayerCode))
            .Returns(new ReadBucketResult(1000M, Guid.NewGuid()));

        _ = fakeServiceCoordinator.QueryAsync<AllAgentsByBrandQuery, AgentInfo>(Arg.Any<AllAgentsByBrandQuery>())
            .Returns(new[]
            {
                new AgentInfo(
                    agentId,
                    expected.AgentUsername,
                    Guid.NewGuid(),
                    AgentStatus.Enabled)
            }.ToAsyncEnumerable());

        _ = fakeCashServiceClient.GetBalanceAsync(Arg.Any<Guid>())
            .Returns(500M);

        _ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(Arg.Is(query.PlayerCode))
            .Returns(new PlayerVipLevelInfoViewModel(
                ResellerId: Guid.NewGuid(),
                ResellerCode: "Demo",
                LicenseeId: Guid.NewGuid(),
                LicenseeCode: "TGS",
                BrandId: Guid.NewGuid(),
                BrandCode: "GPI",
                PlayerCode: "tester",
                Username: "tester",
                VipLevelId: expected.VipId,
                VipLevel: expected.VipLevel,
                AutoState: Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                AccumDepositId: Guid.NewGuid(),
                AccumDeposit: expected.AccumDeposit,
                AccumTurnoverId: Guid.NewGuid(),
                AccumTurnover: expected.AccumTurnover,
                LevelUpAccumulateBetAmount: expected.LevelUpAccumulateBetAmount,
                LevelUpAccumulateDeposit: expected.LevelUpAccumulateDeposit,
                FullLevel: expected.FullLevel,
                NextVipLevelId: expected.NextVipLevelId,
                NextVipLevel: expected.NextVipLevel));

        var actual = await sut.GetAsync(query);

        expected.ToExpectedObject().ShouldMatch(actual);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Player_Operations(bool showAll)
    {
        var mediatorMock = Substitute.For<IMediator>();

        var promotionClientMock = Substitute.For<IPromotionClient>();

        var controller = new PlayerController(mediatorMock, promotionClientMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var playerCode = Generator.RandomString(12);

        var request = new GetPlayerOperationRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "CreatedOn",
            OrderBy = TGS.Core.Models.Messages.SortOrder.Descending,
            ShowAll = showAll
        };

        var fixture = new Fixture();
        var list = fixture
            .CreateMany<PlayerOperationModel>(105)
            .AsQueryable()
            .BuildMock();

        var (skip, take) = GetPage(request);
        var orderedPlayers = list.OrderByDescending(p => p.CreatedOn);

        var expectedPlayers = showAll ? orderedPlayers.ToArray() : orderedPlayers.Skip(skip).Take(take).ToArray();

        _ = mediatorMock
            .Send(Arg.Any<GetPlayerOperationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<PlayerOperationModel>
            {
                Valid = true,
                Items = expectedPlayers
            });

        var response = await controller.GetPlayerOperationsAsync(playerCode, request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].CreatedOn.Should().Be(expectedPlayers[i].CreatedOn);
            _ = response.Items[i].CreatedByUsername.Should().Be(expectedPlayers[i].CreatedByUsername);
            _ = response.Items[i].Operation.Should().Be(expectedPlayers[i].Operation);
        }

        _ = mediatorMock.Received(1).Send(Arg.Any<GetPlayerOperationRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Cannot_Update_Player_Not_Found()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var promotionClientMock = Substitute.For<IPromotionClient>();

        var controller = new PlayerController(mediatorMock, promotionClientMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var playerCode = Generator.RandomString(12);

        _ = mediatorMock
            .Send(Arg.Any<UpdatePlayerRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Valid = false,
                Message = Player.NotFound
            });

        var request = CreateUpdatePlayerRequestFixture()
            .Create<Apis.BackendOffice.Controllers.Messages.Player.UpdatePlayerRequest>();

        var response = await controller.UpdateAsync(playerCode, "info", request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Player.NotFound);

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdatePlayerRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Cannot_Update_Player_With_Invalid_Update_Type()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var promotionClientMock = Substitute.For<IPromotionClient>();

        var controller = new PlayerController(mediatorMock, promotionClientMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var playerCode = Generator.RandomString(12);

        _ = mediatorMock
            .Send(Arg.Any<UpdatePlayerRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Valid = false,
                Message = Player.UpdateTypeNotFound
            });

        var request = CreateUpdatePlayerRequestFixture()
            .Create<Apis.BackendOffice.Controllers.Messages.Player.UpdatePlayerRequest>();

        var response = await controller.UpdateAsync(playerCode, "info2", request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Player.UpdateTypeNotFound);

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdatePlayerRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_Player_Status()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var promotionClientMock = Substitute.For<IPromotionClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeSetPlayerStatus = Substitute.For<IGetService<SetPlayerStatusCommand, SetPlayerStatusResult>>();
        var fakeSetAgentStatus = Substitute.For<IGetService<SetAgentStatusCommand, SetAgentStatusResult>>();

        var controller = new PlayerController(mediatorMock, promotionClientMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var playerCode = Generator.RandomString(12);
        var agentId = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetAgentIdForPlayerUpgradedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(agentId);

        _ = fakeSetPlayerStatus.GetAsync(Arg.Any<SetPlayerStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(new SetPlayerStatusResult(true, ""));

        var request = new UpdatePlayerStatusViewModel { Status = PlayerStatus.Disabled };

        var response = await controller.SetPlayerStatusAsync(fakePlayerServiceClient,
            fakeSetPlayerStatus,
            fakeSetAgentStatus,
            playerCode,
            request);

        _ = response.Valid.Should().BeTrue();

        _ = fakeSetPlayerStatus.ReceivedWithAnyArgs(1).GetAsync(null!, default);

        _ = fakeSetAgentStatus.ReceivedWithAnyArgs(1).GetAsync(null!, default);
    }

    [Test]
    public async Task Can_Update_Player_Password()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var promotionClientMock = Substitute.For<IPromotionClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeSetPlayerPassword = Substitute.For<IGetService<SetPlayerPasswordCommand, SetPlayerPasswordInfo>>();
        var fakeSetAgentPassword = Substitute.For<IGetService<SetAgentPasswordCommand, SetAgentPasswordResult>>();

        var controller = new PlayerController(mediatorMock, promotionClientMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var playerCode = Generator.RandomString(12);
        var agentId = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetAgentIdForPlayerUpgradedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(agentId);

        _ = fakeSetPlayerPassword.GetAsync(Arg.Any<SetPlayerPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(new SetPlayerPasswordInfo(true, ""));

        var response = await controller.SetPlayerPasswordAsync(fakePlayerServiceClient,
            fakeSetPlayerPassword,
            fakeSetAgentPassword,
            playerCode,
            "password");

        _ = response.Valid.Should().BeTrue();

        _ = fakeSetPlayerPassword.ReceivedWithAnyArgs(1).GetAsync(null!, default);

        _ = fakeSetAgentPassword.ReceivedWithAnyArgs(1).GetAsync(null!, default);
    }

    [Test]
    public async Task Can_Upgrade_Player_To_Agent()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var promotionClientMock = Substitute.For<IPromotionClient>();

        var controller = new PlayerController(mediatorMock, promotionClientMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var playerCode = Generator.RandomString(12);

        _ = mediatorMock
            .Send(Arg.Any<UpdatePlayerRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Valid = false,
                Message = Player.UpdateTypeNotFound
            });

        var request = CreateUpdatePlayerRequestFixture()
            .Create<Apis.BackendOffice.Controllers.Messages.Player.UpdatePlayerRequest>();

        var response = await controller.UpdateAsync(playerCode, "info2", request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Player.UpdateTypeNotFound);

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdatePlayerRequest>(), Arg.Any<CancellationToken>());
    }

    private static Fixture CreateUpdatePlayerRequestFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Apis.BackendOffice.Controllers.Messages.Player.UpdatePlayerRequest>(u => u
            .With(s => s.Birthdate, new DateOnly()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
