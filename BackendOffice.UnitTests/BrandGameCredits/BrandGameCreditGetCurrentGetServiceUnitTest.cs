using NUnit.Framework.Legacy;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common;
using Tgs.BrandGameCredits.Common.Models;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

namespace TGS.Tests.BackendOffice.UnitTests.BrandGameCredits;

public class BrandGameCreditGetCurrentGetServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();

        var query = new BrandGameCreditGetCurrentQuery(
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
            .GameCreditGetCurrentAsync(
            brandCode: Arg.Is(brandInfo.Code),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(brandGameCredit);

        var service = new BrandGameCreditGetCurrentGetService(
            fakeBrandServiceClient,
            fakeBrandGameCreditsClient);

        var actual = await service.GetAsync(query);

        ClassicAssert.NotNull(actual);
        ClassicAssert.AreEqual(Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"), actual!.ResellerId);
        ClassicAssert.AreEqual("TGS", actual.ResellerCode);
        ClassicAssert.AreEqual(Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"), actual.LicenseeId);
        ClassicAssert.AreEqual("TGS", actual.LicenseeCode);
        ClassicAssert.AreEqual(Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"), actual.BrandId);
        ClassicAssert.AreEqual("GPI", actual.BrandCode);
        ClassicAssert.AreEqual(DateOnly.Parse("2023-12-01"), actual.Month);
        ClassicAssert.AreEqual(2m, actual.InitialAmount);
        ClassicAssert.AreEqual(100m, actual.CreditAmount);
        ClassicAssert.AreEqual(1, actual.DebitAmount);
        ClassicAssert.AreEqual(-33m, actual.ProfitAmount);
        ClassicAssert.AreEqual(101m, actual.CreditLimit);
        ClassicAssert.AreEqual(33m, actual.CreditUsed);
        ClassicAssert.AreEqual(32.6733m, actual.CreditUsedPer);
        ClassicAssert.AreEqual(68m, actual.CreditAvailable);
        ClassicAssert.AreEqual(67.3267m, actual.CreditAvailablePer);
        ClassicAssert.AreEqual(BrandGameCreditRiskTypes.NormalRisk, actual.CreditRiskType);
        ClassicAssert.AreEqual(DateTimeOffset.Parse("2023-12-06T09:19:50.010516+00:00"), actual.CreatedOn);
        ClassicAssert.AreEqual(DateTimeOffset.Parse("2023-12-07T10:38:10.350982+00:00"), actual.UpdatedOn);
    }
}
