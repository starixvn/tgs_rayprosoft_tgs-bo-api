using NUnit.Framework.Legacy;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common.Models;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

namespace TGS.Tests.BackendOffice.UnitTests.BrandGameCredits;

public class BrandGameCreditProfitQueryServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();

        var query = new BrandGameCreditProfitQuery(
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            Month: DateOnly.Parse("2023-12-01"));

        var brandInfo = new BrandInfo
        {
            Id = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            Code = "GPI",
            Status = BrandStatus.Active,
            Currency = "CNY",
            SmsEnabled = false,
            SmsRegisterFlowEnabled = false,
            PlayerIdentityEnabled = false,
            EmailRegisterFlowEnabled = false,
            BankInfoRegisterFlowEnabled = false,
            TimeSpanMinutes = 480
        };
        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            brandId: Arg.Is(query.BrandId),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(brandInfo);

        var brandGameCreditProfit = new BrandGameCreditProfit(
            ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
            ResellerCode: "TGS",
            LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
            LicenseeCode: "TGS",
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            BrandCode: "GPI",
            Month: DateOnly.Parse("2023-12-01"),
            GameProviderId: Guid.Parse("6214260a-cf3c-4fb5-ba78-9214fe5608bf"),
            GameProviderCode: "AWC_PGS",
            GameProviderName: "PGSoft",
            Count: 20,
            Bet: 10m,
            ValidBet: 10m,
            Settle: 9.7m,
            Commission: 0m,
            Turnover: 10m,
            WinLoss: -0.3m,
            ProfitAmount: 0.3m);
        _ = fakeBrandGameCreditsClient
            .GameCreditProfitQueryAsync(
            brandCode: Arg.Is(brandInfo.Code),
            month: Arg.Is(query.Month),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new BrandGameCreditProfit[] { brandGameCreditProfit }.ToAsyncEnumerable());

        var service = new BrandGameCreditProfitQueryService(
            fakeBrandServiceClient,
            fakeBrandGameCreditsClient);

        var actual = await service.QueryAsync(query)
            .ToArrayAsync();

        ClassicAssert.True(actual.Length != 0);
        ClassicAssert.AreEqual(Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"), actual.First()!.ResellerId);
        ClassicAssert.AreEqual("TGS", actual.First()!.ResellerCode);
        ClassicAssert.AreEqual(Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"), actual.First()!.LicenseeId);
        ClassicAssert.AreEqual("TGS", actual.First()!.LicenseeCode);
        ClassicAssert.AreEqual(Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"), actual.First()!.BrandId);
        ClassicAssert.AreEqual("GPI", actual.First()!.BrandCode);
        ClassicAssert.AreEqual(DateOnly.Parse("2023-12-01"), actual.First()!.Month);
        ClassicAssert.AreEqual(Guid.Parse("6214260a-cf3c-4fb5-ba78-9214fe5608bf"), actual.First().GameProviderId);
        ClassicAssert.AreEqual(20, actual.First().Count);
        ClassicAssert.AreEqual(10m, actual.First().Bet);
        ClassicAssert.AreEqual(10m, actual.First().ValidBet);
        ClassicAssert.AreEqual(9.7m, actual.First().Settle);
        ClassicAssert.AreEqual(0m, actual.First().Commission);
        ClassicAssert.AreEqual(10, actual.First().Turnover);
        ClassicAssert.AreEqual(-0.3m, actual.First().WinLoss);
        ClassicAssert.AreEqual(0.3m, actual.First().ProfitAmount);
    }
}
