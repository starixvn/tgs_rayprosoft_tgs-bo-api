using System.Text.Json.Nodes;

using NSubstitute;
using NUnit.Framework.Legacy;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common;
using Tgs.BrandGameCredits.Common.Models;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

namespace TGS.Tests.BackendOffice.UnitTests.BrandGameCredits;

public class BrandGameCreditTxnQueryServiceUnitTest
{
    [Test]
    public async Task Query()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();

        var query = new BrandGameCreditTxnQuery(
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            StartOn: DateTimeOffset.UtcNow.AddHours(-1),
            EndOn: DateTimeOffset.UtcNow,
            Type: null,
            PageNumber: 1,
            PageSize: 20);

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

        var txn = new BrandGameCreditTxn(
            Id: Guid.NewGuid(),
            ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
            ResellerCode: "TGS",
            LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
            LicenseeCode: "TGS",
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            BrandCode: "GPI",
            Type: BrandGameCreditTxnTypes.Credit,
            Money: 100m,
            Amount: 1000m,
            Remark: "text",
            Expansions: new JsonObject { ["User"] = "test" },
            CreatedUserId: Guid.Parse("50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5"),
            CreatedUsername: "admin.enabled",
            CreatedOn: DateTimeOffset.Parse("2023-12-06T10:23:22.533+08:00"));
        _ = fakeBrandGameCreditsClient
            .GameCreditTxnQueryAsync(
            brandCode: Arg.Is(brandInfo.Code),
            startOn: Arg.Is(query.StartOn),
            endOn: Arg.Is(query.EndOn),
            type: Arg.Is(query.Type),
            pageNumber: Arg.Is(query.PageNumber),
            pageSize: Arg.Is(query.PageSize),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new BrandGameCreditTxn[] { txn }.ToAsyncEnumerable());

        var service = new BrandGameCreditTxnQueryService(
            fakeBrandServiceClient,
            fakeBrandGameCreditsClient);

        var actual = await service.QueryAsync(query)
            .ToArrayAsync();

        ClassicAssert.True(actual.Length != 0);
        ClassicAssert.AreEqual(Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"), actual.First().ResellerId);
        ClassicAssert.AreEqual("TGS", actual.First().ResellerCode);
        ClassicAssert.AreEqual(Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"), actual.First().LicenseeId);
        ClassicAssert.AreEqual("TGS", actual.First().LicenseeCode);
        ClassicAssert.AreEqual(Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"), actual.First().BrandId);
        ClassicAssert.AreEqual("GPI", actual.First().BrandCode);
        ClassicAssert.AreEqual(BrandGameCreditTxnTypes.Credit, actual.First().Type);
        ClassicAssert.AreEqual(100m, actual.First().Money);
        ClassicAssert.AreEqual(1000m, actual.First().Amount);
        ClassicAssert.AreEqual("text", actual.First().Remark);
        ClassicAssert.AreEqual(new JsonObject { ["User"] = "test" }.ToJsonString(), actual.First().Expansions.ToJsonString());
        ClassicAssert.AreEqual(Guid.Parse("50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5"), actual.First().CreatedUserId);
        ClassicAssert.AreEqual("admin.enabled", actual.First().CreatedUsername);
        ClassicAssert.AreEqual(DateTimeOffset.Parse("2023-12-06T10:23:22.533+08:00"), actual.First().CreatedOn);
    }

    [Test]
    public async Task QueryCount()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();

        var query = new BrandGameCreditTxnQuery(
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            StartOn: DateTimeOffset.UtcNow.AddHours(-1),
            EndOn: DateTimeOffset.UtcNow,
            Type: null,
            PageNumber: 1,
            PageSize: 20);

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

        var count = 10;
        _ = fakeBrandGameCreditsClient
            .GameCreditTxnQueryCountAsync(
            brandCode: Arg.Is(brandInfo.Code),
            startOn: Arg.Is(query.StartOn),
            endOn: Arg.Is(query.EndOn),
            type: Arg.Is(query.Type),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(count);

        var service = new BrandGameCreditTxnQueryService(
            fakeBrandServiceClient,
            fakeBrandGameCreditsClient);

        var actual = await service.GetAsync(query);

        ClassicAssert.AreEqual(count, actual);
    }
}
