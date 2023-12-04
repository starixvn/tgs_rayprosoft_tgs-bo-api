using System.Text.Json;
using ExpectedObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework.Legacy;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests.DataServices;

[TestFixture]
public class BrandPaymentServiceTests
{
    [Test]
    public async Task Update_Payment_Should_Log_Audit_Data()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        ITgsRepository repository = new TgsRepository(dbContextOptionsBuilder.Options);

        var source = new BrandPayment
        {
            PaymentMethodChannelId = Guid.Parse("30cb4d17-2506-4d72-8470-65063b80f9c9"),
            ChannelName = "ChannelNass888",
            TransferLimitFrom = 55,
            TransferLimitTo = 55,
            PlayerListCondition = PlayerListCondition.MatchAll,
            AllPlayers = true,
            IdentityAuthentication = true,
            PhoneAuthentication = true,
            DepositedAmountUnlimited = true,
            DepositedAmountMax = 60000,
            DepositedAmountMin = 33,
            Reason = "source reason",
            BrandPaymentPlayers = Array.Empty<BrandPaymentPlayer>(),
            VipLevels = Array.Empty<BrandPaymentVipLevel>(),
            ExtendedData = "{}",
            CreatedByUsername = "System",
            UpdatedByUsername = "System"
        };

        _ = repository.BrandPayments.Add(source);
        _ = await repository.SaveChangesAsync();

        var request = new UpdateBrandPaymentRequest
        {
            PaymentMethodChannelId = Guid.Parse("30cb4d17-2506-4d72-8470-65063b80f9c9"),
            ChannelName = "ChannelNass9",
            TransferLimitFrom = 33,
            TransferLimitTo = 33,
            PlayerListCondition = PlayerListCondition.MatchAll,
            AllPlayers = true,
            IdentityAuthentication = true,
            PhoneAuthentication = true,
            DepositedAmountUnlimited = true,
            DepositedAmountMax = 50000,
            DepositedAmountMin = 55,
            Reason = "this is reason",
            Whitelist = new[] { "4uc3t7zw2k38", "4uc3t7zw2zo1" },
            VipLevelIds = new[] {
                Guid.Parse("0EAA7EC5-771E-EB11-8D6A-70B5E82B6B7C"),
                Guid.Parse("0FAA7EC5-771E-EB11-8D6A-70B5E82B6B7C")
            }
        };

        IBrandPaymentService sut = new BrandPaymentService(repository);

        var beforeExpected = new BrandPaymentAuditData(
            source.ChannelName,
            source.PaymentStatus,
            source.TransferLimitFrom,
            source.TransferLimitTo,
            source.PlayerListCondition,
            source.AllPlayers,
            source.IdentityAuthentication,
            source.PhoneAuthentication,
            source.DepositedAmountMin,
            source.DepositedAmountMax,
            source.DepositedAmountUnlimited).ToExpectedObject();

        var afterExpected = new BrandPaymentAuditData(
            request.ChannelName,
            request.PaymentStatus,
            request.TransferLimitFrom,
            request.TransferLimitTo,
            request.PlayerListCondition,
            request.AllPlayers,
            request.IdentityAuthentication,
            request.PhoneAuthentication,
            request.DepositedAmountMin,
            request.DepositedAmountMax,
            request.DepositedAmountUnlimited).ToExpectedObject();

        await sut.UpdatePaymentAsync(source, request, "tester");

        var logs = await repository.PaymentAuditLogs.AsQueryable().ToArrayAsync();

        ClassicAssert.AreEqual(1, logs.Length);

        var auditLog = logs.First();
        var beforeActual = JsonSerializer.Deserialize<BrandPaymentAuditData>(auditLog.Before);
        var afterActual = JsonSerializer.Deserialize<BrandPaymentAuditData>(auditLog.After);

        beforeExpected.ShouldMatch(beforeActual);
        afterExpected.ShouldMatch(afterActual);
    }
}
