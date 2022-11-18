using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.DepositAccumulateRoulette.Clients;
using Tgs.Promotion.Clients;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

internal class DepositAccumulateRouletteTests
{
    [Test]
    public async Task Can_Get_Roulette_Record_Detail()
    {
        var fakeRouletteServiceClient = Substitute.For<IDepositAccumulateRouletteServiceClient>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var brandId = Guid.NewGuid();

        var rouletteRecord = new RouletteRecord(
            Guid.NewGuid(),
            "player1,",
        Guid.NewGuid(),
            DateOnly.Parse("2022-11-18"),
            1,
            DateTimeOffset.Parse("2022-11-18 10:00:00+08:00"));

        var rouletteSettings = new DepositAccumulateRouletteSettings
        {
            CampaignDetail = new DepositAccumulateRouletteCollection
            {
                RouletteItems = new[]
               {
                   new DepositAccumulateRouletteItem
                   {
                       Id = Guid.NewGuid().ToString(),
                       Title = "prize1"
                   },
                   new DepositAccumulateRouletteItem
                   {
                       Id = Guid.NewGuid().ToString(),
                       Title = "prize2"
                   },
                   new DepositAccumulateRouletteItem
                   {
                       Id = Guid.NewGuid().ToString(),
                       Title = "prize3"
                   }
               }
            }
        };

        var expected = new DepositAccumulateRouletteGameRecordDetailInfo("prize2");

        _ = fakeRouletteServiceClient.GetRouletteRecordAsync(
                Arg.Is(brandId),
                Arg.Is(rouletteRecord.RouletteId),
                Arg.Any<CancellationToken>())
            .Returns(rouletteRecord);

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<DepositAccumulateRouletteSettings>(
            Arg.Is(rouletteRecord.PromotionId),
            Arg.Any<CancellationToken>())
            .Returns(rouletteSettings);

        var sut = new DepositAccumulateRouletteRecordDetailQueryService(
            fakeRouletteServiceClient,
            fakePromotionServiceClient);

        var actual = await sut.GetAsync(
            new DepositAccumulateRouletteRecordDetailQuery(brandId, rouletteRecord.RouletteId));

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected, actual);
    }
}
