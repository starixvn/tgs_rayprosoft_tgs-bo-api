using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework.Legacy;
using TGS.Core.DataServices.Tgs;
using TGS.Services.BackOffice.Application.Queries.Game;
using TGS.Services.BackOffice.Domain;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class GamesLocalizationTests
{
    [Test]
    public async Task Game_Languages_Count_Should_Match_GameProvider_Language_Count()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        var repository = new TgsRepository(dbContextOptionsBuilder.Options);

        var gameProviderId = Guid.NewGuid();
        var gameProviderCode = "KY";
        _ = await repository.GameProviders.AddAsync(
            new CoreModels.GameProvider()
            {
                Id = gameProviderId,
                Code = gameProviderCode,
                Name = "KY",
                CreatedByUsername = "System",
                UpdatedByUsername = "System"
            });

        var gpLangs = new List<CoreModels.GameProviderLanguage>
        {
            new()
            {
                GameProviderId = gameProviderId,
                CultureCode = "zh-CN",
                GameProviderCultureCode = "zh-CN"
            },
            new()
            {
                GameProviderId = gameProviderId,
                CultureCode = "en-US",
                GameProviderCultureCode = "en-US"
            }
        };

        await repository.GameProviderLanguages.AddRangeAsync(gpLangs);

        _ = await repository.SaveChangesAsync();

        var sut = new GetGameLocalizationQueryService(repository);

        var query = new GetGameLocalizationQuery(gameProviderCode);
        var info = await sut.GetAsync(query, default);

        ClassicAssert.AreEqual(gpLangs.Count, info.Languages.Count());
    }

    [Test]
    public async Task Update_Game_Prodvier_Location_With_Exist_Id_Should_Success()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        var repository = new TgsRepository(dbContextOptionsBuilder.Options);

        _ = repository.Games.Add(new CoreModels.Game()
        {
            Id = 1,
            AvailableCurrencies = string.Empty,
            Code = "Game1",
            Name = "Game1",
            ExternalId = "1",
            CreatedByUsername = "System",
            UpdatedByUsername = "System"
        });
        _ = repository.Games.Add(new CoreModels.Game()
        {
            Id = 2,
            AvailableCurrencies = string.Empty,
            Code = "Game2",
            Name = "Game2",
            ExternalId = "2",
            CreatedByUsername = "System",
            UpdatedByUsername = "System"
        });
        _ = await repository.SaveChangesAsync();

        var sut = new UpdateGameLocalizationQueryService(repository);

        var games = new List<GameLocalizationModel>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };

        var query = new UpdateGameLocalizationQuery()
        {
            Games = games
        };

        var info = await sut.GetAsync(query, default);

        ClassicAssert.AreEqual(0, info.DiffGameIds.Length);
    }

    [Test]
    public async Task Update_Game_Provider_Localization_With_NonExist_Id_Should_Failed()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        var repository = new TgsRepository(dbContextOptionsBuilder.Options);

        _ = repository.Games.Add(new CoreModels.Game()
        {
            Id = 1,
            AvailableCurrencies = string.Empty,
            Code = "Game1",
            Name = "Game1",
            ExternalId = "1",
            CreatedByUsername = "System",
            UpdatedByUsername = "System"
        });
        _ = repository.Games.Add(new CoreModels.Game()
        {
            Id = 2,
            AvailableCurrencies = string.Empty,
            Code = "Game2",
            Name = "Game2",
            ExternalId = "2",
            CreatedByUsername = "System",
            UpdatedByUsername = "System"
        });

        _ = await repository.SaveChangesAsync();

        var sut = new UpdateGameLocalizationQueryService(repository);

        var games = new List<GameLocalizationModel>
        {
            new() { Id = 3 },
            new() { Id = 4 }
        };

        var query = new UpdateGameLocalizationQuery()
        {
            Games = games
        };

        var info = await sut.GetAsync(query, default);

        var actual = info.DiffGameIds;

        ClassicAssert.AreNotEqual(0, actual);
    }
}
