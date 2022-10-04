using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.BrandCertificate.Clients;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.GameProviders.Pluggable.ViewModels;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests
{
    [TestFixture]
    public class RoundReportTests
    {
        [Test]
        public async Task Add_Round_Should_Success()
        {
            var playerServiceClient = Substitute.For<IPlayerServiceClient>();
            var pluggableClient = Substitute.For<IGameProvidersBackofficePluggableClient>();
            var brandServiceClient = Substitute.For<IBrandServiceClient>();

            var playerCode = "4ucmqplxb029";
            var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

            _ = playerServiceClient
                .GetPlayerInfoAsync(playerCode)
                .Returns(new PlayerInfo { BrandId = brandId });

            var playerProps = new Dictionary<string, string>
            {
                { "DefaultWalletId", Guid.NewGuid().ToString() }
            };

            _ = playerServiceClient
                .GetPlayerPropertiesAsync(playerCode)
                .Returns(playerProps.ToImmutableDictionary());

            _ = brandServiceClient
                .GetBrandInfoAsync(brandId)
                .Returns(new BrandInfo { Id = brandId, Currency = "CNY" });

            var sut = new AddCompanyRoundCommandService(playerServiceClient, pluggableClient, brandServiceClient);

            var command = new AddCompanyRoundCommand
            {
                PlayerCode = playerCode,
                BetAmount = 4,
                ValidBetAmount = 4,
                SettleAmount = 0,
                Commission = 0,
                ClientRoundStatus = GameRoundClosedClientRoundStatus.Win,
                CompanyWinLoss = 4,
                Turnover = 4,
                ExternalCreatedOn = DateTimeOffset.Parse("2022-02-18 11:02:48"),
                ExternalClosedOn = DateTimeOffset.Parse("2022-02-18 11:03:16"),
                ExternalRoundId = "50-1645153368-72600295-2",
                GameId = 1000017,
                GameProviderId = Guid.Parse("a0f49ce0-2c1d-4f50-9fc5-58b610a30bf5"),

                OperatorId = "47d6a809-8b90-ec11-a746-ac82478e2af1",
                Remark = "remark",
            };
            await sut.ExecuteAsync(command);

            await pluggableClient
                .Received(1)
                .AddCompanyRoundAsync(Arg.Any<AddCompanyRoundViewModel>());
        }
    }
}
