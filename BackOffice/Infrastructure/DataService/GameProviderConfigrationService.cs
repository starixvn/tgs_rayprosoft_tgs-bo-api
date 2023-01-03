using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IGameProviderConfigurationService
{
    Task CreateGameProviderConfigurationAsync(CreateGameProviderConfigurationRequest request);

    Task<bool> ExistsAsync(Expression<Func<GameProviderConfiguration, bool>> predicate);

    Task<GameProviderConfiguration> GetGameProviderConfigurationAsync(
                Expression<Func<GameProviderConfiguration, bool>> predicate,
        Func<IQueryable<GameProviderConfiguration>, IQueryable<GameProviderConfiguration>> preQuery =
            null);

    IQueryable<GameProviderConfiguration> GetGameProviderConfigurationQueries(
        Func<IQueryable<GameProviderConfiguration>, IQueryable<GameProviderConfiguration>> preQuery = null,
        bool tracking = false);

    Task UpdateGameProviderConfigurationAsync(GameProviderConfiguration configuration,
        UpdateGameProviderConfigurationRequest request);
}

public class GameProviderConfigurationService : BaseRepositoryOperations<ITgsRepository>,
    IGameProviderConfigurationService
{
    public GameProviderConfigurationService(ITgsRepository tgsRepository) : base(tgsRepository)
    {
    }

    async Task IGameProviderConfigurationService.CreateGameProviderConfigurationAsync(
        CreateGameProviderConfigurationRequest request)
    {
        var configuration = new GameProviderConfiguration
        {
            Name = request.Name,
            Description = request.Description,
            ClientId = request.ClientId,
            SecretKey = request.SecretKey,
            Status = Status.Active,
            GameProviderId = request.GameProviderId,
            DesktopEndpoint = request.DesktopEndpoint,
            MobileEndpoint = request.MobileEndpoint,
            ConfigurationData = request.ConfigurationData,
            IsDefault = request.IsDefault,
            CreatedByUsername = request.EditorUsername,
            UpdatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        await CreateEntityAsync(configuration);
    }

    Task<bool> IGameProviderConfigurationService.ExistsAsync(
        Expression<Func<GameProviderConfiguration, bool>> predicate) =>
        ExistsAsync(predicate);

    Task<GameProviderConfiguration> IGameProviderConfigurationService.GetGameProviderConfigurationAsync(
                Expression<Func<GameProviderConfiguration, bool>> predicate,
        Func<IQueryable<GameProviderConfiguration>, IQueryable<GameProviderConfiguration>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<GameProviderConfiguration> IGameProviderConfigurationService.
        GetGameProviderConfigurationQueries(
            Func<IQueryable<GameProviderConfiguration>, IQueryable<GameProviderConfiguration>> preQuery,
            bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    async Task IGameProviderConfigurationService.UpdateGameProviderConfigurationAsync(
        GameProviderConfiguration configuration,
        UpdateGameProviderConfigurationRequest request)
    {
        configuration.Name = request.Name;
        configuration.Description = request.Description;
        configuration.ClientId = request.ClientId;
        configuration.SecretKey = request.SecretKey;
        configuration.Status = request.Status;
        configuration.GameProviderId = request.GameProviderId;
        configuration.DesktopEndpoint = request.DesktopEndpoint;
        configuration.MobileEndpoint = request.MobileEndpoint;
        configuration.ConfigurationData = request.ConfigurationData;
        configuration.IsDefault = request.IsDefault;
        configuration.UpdatedByUsername = request.EditorUsername;
        configuration.UpdatedOn = DateTimeOffset.Now;

        await UpdateEntityAsync(configuration);
    }
}
