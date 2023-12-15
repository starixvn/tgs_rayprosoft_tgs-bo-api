using NSubstitute;
using NUnit.Framework.Legacy;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common;
using Tgs.BrandGameCredits.Common.Models;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

namespace TGS.Tests.BackendOffice.UnitTests.BrandGameCredits;

public class BrandGameCreditQueryServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();

        var query = new BrandGameCreditQuery(
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"));

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

        var brandGameCredit = new BrandGameCredit(
            ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
            ResellerCode: "TGS",
            LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
            LicenseeCode: "TGS",
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            BrandCode: "GPI",
            Month: DateOnly.Parse("2023-12-01"),
            InitialAmount: 2m,
            CreditAmount: 100m,
            DebitAmount: 1m,
            ProfitAmount: -33m,
            CreditLimit: 101m,
            CreditUsed: 33m,
            CreditUsedPer: 32.6733m,
            CreditAvailable: 68m,
            CreditAvailablePer: 67.3267m,
            BrandGameCreditRiskTypes.NormalRisk,
            CreatedOn: DateTimeOffset.Parse("2023-12-06T09:19:50.010516+00:00"),
            UpdatedOn: DateTimeOffset.Parse("2023-12-07T10:38:10.350982+00:00"));
        _ = fakeBrandGameCreditsClient
            .GameCreditQueryAsync(
            brandCode: Arg.Is(brandInfo.Code),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new BrandGameCredit[] { brandGameCredit }.ToAsyncEnumerable());

        var service = new BrandGameCreditQueryService(
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
        ClassicAssert.AreEqual(2m, actual.First()!.InitialAmount);
        ClassicAssert.AreEqual(100m, actual.First()!.CreditAmount);
        ClassicAssert.AreEqual(1, actual.First()!.DebitAmount);
        ClassicAssert.AreEqual(-33m, actual.First()!.ProfitAmount);
        ClassicAssert.AreEqual(101m, actual.First()!.CreditLimit);
        ClassicAssert.AreEqual(33m, actual.First()!.CreditUsed);
        ClassicAssert.AreEqual(32.6733m, actual.First()!.CreditUsedPer);
        ClassicAssert.AreEqual(68m, actual.First()!.CreditAvailable);
        ClassicAssert.AreEqual(67.3267m, actual.First()!.CreditAvailablePer);
        ClassicAssert.AreEqual(BrandGameCreditRiskTypes.NormalRisk, actual.First()!.CreditRiskType);
        ClassicAssert.AreEqual(DateTimeOffset.Parse("2023-12-06T09:19:50.010516+00:00"), actual.First()!.CreatedOn);
        ClassicAssert.AreEqual(DateTimeOffset.Parse("2023-12-07T10:38:10.350982+00:00"), actual.First()!.UpdatedOn);
    }
}
