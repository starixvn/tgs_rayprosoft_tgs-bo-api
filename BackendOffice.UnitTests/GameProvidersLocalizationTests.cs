using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework.Legacy;
using TGS.Core.DataServices.Tgs;
using TGS.Core.Infrastructures.Json;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;
using TGS.Services.BackOffice.Application.Queries.GameProviders;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class GameProvidersLocalizationTests
{
    [Test]
    public async Task Can_Query_SubGameProvider_Data()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        var repository = new TgsRepository(dbContextOptionsBuilder.Options);

        await repository.SubGameProviders.AddRangeAsync(
            new CoreModels.SubGameProvider
            {
                Code = "Test",
                CreatedByUsername = "System",
                UpdatedByUsername = "System",
                Name = "Test"
            });
        _ = await repository.SaveChangesAsync();

        var sut = new GetGameProvidersLocalizationQueryService(repository);

        var actual = await sut.QueryAsync(new GetGameProvidersLocalizationQuery()).CountAsync();

        ClassicAssert.AreEqual(1, actual);
    }

    [Test]
    public async Task GameProviders_Language_Should_Get_Distinct_Culture_Code_Count()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        var repository = new TgsRepository(dbContextOptionsBuilder.Options);

        await repository.GameProviderLanguages.AddRangeAsync(
            new CoreModels.GameProviderLanguage()
            {
                GameProviderId = Guid.NewGuid(),
                CultureCode = "en-US",
                GameProviderCultureCode = "en-US"
            },
            new CoreModels.GameProviderLanguage()
            {
                GameProviderId = Guid.NewGuid(),
                CultureCode = "en-US",
                GameProviderCultureCode = "en-US"
            },
            new CoreModels.GameProviderLanguage()
            {
                GameProviderId = Guid.NewGuid(),
                CultureCode = "zh-CN",
                GameProviderCultureCode = "zh-CN"
            });
        _ = await repository.SaveChangesAsync();

        var sut = new GetAllSupportLanguagesQueryService(repository);

        var actual = await sut.QueryAsync(new GetAllSupportLanguagesQuery()).CountAsync();

        ClassicAssert.AreEqual(2, actual);
    }

    [Test]
    public async Task Update_Game_Prodvier_Location_With_Exist_Id_Should_Success()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        var repository = new TgsRepository(dbContextOptionsBuilder.Options);

        _ = await repository.SubGameProviders.AddAsync(
            new CoreModels.SubGameProvider()
            {
                Code = "SubGp",
                Name = "SubGp",
                CreatedByUsername = "System",
                UpdatedByUsername = "System"
            });
        _ = await repository.SaveChangesAsync();

        var sut = new UpdateGameProvidersLocalizationQueryService(
            repository,
            new JsonSerializationProvider());

        var localization = new Dictionary<string, JsonDocument>()
        {
            { "en-US", JsonDocument.Parse("{ \"name\": \"Happy\" }") },
            { "zh-CN", JsonDocument.Parse("{ \"name\": \"開心\" }") },
            { "zh-TW", JsonDocument.Parse("{ \"name\": \"开心\" }") }
        };

        var command = new UpdateGameProvidersLocalizationCommand(
            [
                new() {
                    Code = "SubGp",
                    Localization = localization
                }],
            "Tester");

        _ = await sut.QueryAsync(command, default).CountAsync();

        var actual = repository.SubGameProviders.FirstOrDefault(x => x.Code == "SubGp");

        ClassicAssert.IsNotNull(actual!.Localization);
    }

    [Test]
    public async Task Update_Game_Provider_Localization_With_NonExist_Id_Should_Failed()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        var repository = new TgsRepository(dbContextOptionsBuilder.Options);

        var sut = new UpdateGameProvidersLocalizationQueryService(
            repository,
            new JsonSerializationProvider());

        var localization = new Dictionary<string, JsonDocument>()
        {
            { "en-US", JsonDocument.Parse("{ \"name\": \"Happy\" }") },
            { "zh-CN", JsonDocument.Parse("{ \"name\": \"開心\" }") },
            { "zh-TW", JsonDocument.Parse("{ \"name\": \"开心\" }") }
        };

        var command = new UpdateGameProvidersLocalizationCommand(
            [
                new() {
                    Code = "SubGp",
                    Localization = localization
                }],
            "Tester");

        var actual = await sut.QueryAsync(command, default).CountAsync();

        ClassicAssert.IsTrue(actual == 1);
    }
}
