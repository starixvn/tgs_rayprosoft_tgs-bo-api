using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Apis.BackendOffice.Controllers.Messages.Reseller;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Infrastructure.RedisServices;

namespace TGS.Apis.BackendOffice.DataService.Services;

public interface IResellerService
{
    Task<Reseller> GetResellerAsync(Expression<Func<Reseller, bool>> predicate,
        Func<IQueryable<Reseller>, IQueryable<Reseller>> preQuery = null);

    IQueryable<Reseller> GetResellers(GetResellersRequest request, bool tracking = false);
    Task CreateResellerAsync(CreateResellerRequest request, string username);
    Task UpdateResellerAsync(Reseller reseller, UpdateResellerRequest request, string username);
    Task ChangeStatusAsync(Reseller reseller, Status status, string username);
    Task<bool> ExistsAsync(Expression<Func<Reseller, bool>> predicate);
}

public class ResellerService : IResellerService
{
    private readonly ITgsRepository _repository;
    private readonly IMaintenanceRedisService _maintenanceRedisService;

    public ResellerService(
        ITgsRepository repository,
        IMaintenanceRedisService maintenanceRedisService
        )
    {
        _repository = repository;
        _maintenanceRedisService = maintenanceRedisService ?? throw new ArgumentNullException(nameof(maintenanceRedisService));
    }

    async Task IResellerService.CreateResellerAsync(CreateResellerRequest request,
        string username)
    {
        var reseller = new Reseller
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Status = Status.Active,
            BusinessType = request.BusinessType,
            Description = request.Description,
            OperationalEmail = request.OperationalEmail,
            CreatedByUsername = username,
            UpdatedByUsername = username,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        reseller.Languages = GenerateResellerLanguages(reseller, request);
        reseller.Currencies = GenerateResellerCurrencies(reseller, request);
        reseller.GameProviders = GenerateResellerGameProviders(reseller, request);
        reseller.BetLimits = GenerateResellerBetLimits(reseller, request);

        await _repository.Resellers.AddAsync(reseller).ConfigureAwait(false);
        await _repository.SaveChangesAsync().ConfigureAwait(false);
    }

    Task<Reseller> IResellerService.GetResellerAsync(Expression<Func<Reseller, bool>> predicate,
        Func<IQueryable<Reseller>, IQueryable<Reseller>> preQuery)
    {
        preQuery ??= c => c;

        return preQuery(_repository.Resellers).FirstOrDefaultAsync(predicate);
    }

    Task IResellerService.UpdateResellerAsync(Reseller reseller, UpdateResellerRequest request,
        string username)
    {
        _repository.ResellerLanguages.RemoveRange(reseller.Languages);
        _repository.ResellerCurrencies.RemoveRange(reseller.Currencies);
        _repository.ResellerGameProviders.RemoveRange(reseller.GameProviders);
        _repository.ResellerBetLimits.RemoveRange(reseller.BetLimits);

        reseller.Code = request.Code;
        reseller.Name = request.Name;
        reseller.Description = request.Description;
        reseller.OperationalEmail = request.OperationalEmail;
        reseller.UpdatedByUsername = username;
        reseller.UpdatedOn = DateTimeOffset.Now;
        reseller.Languages = GenerateResellerLanguages(reseller, request);
        reseller.Currencies = GenerateResellerCurrencies(reseller, request);
        reseller.GameProviders = GenerateResellerGameProviders(reseller, request);
        reseller.BetLimits = GenerateResellerBetLimits(reseller, request);

        _repository.Entry(reseller).State = EntityState.Modified;

        return _repository.SaveChangesAsync();
    }

    async Task IResellerService.ChangeStatusAsync(Reseller reseller, Status status, string username)
    {
        reseller.Status = status;
        reseller.UpdatedOn = DateTimeOffset.Now;
        reseller.UpdatedByUsername = username;

        _repository.Entry(reseller).State = EntityState.Modified;

        await _repository.SaveChangesAsync().ConfigureAwait(false);

        if (status is Status.Active or Status.Maintenance)
        {
            var licensees = _repository.Licensees.AsQueryable().Where(x => x.ResellerId == reseller.Id && x.Status == Status.Active);
            var brands = (from brand in _repository.Brands.AsQueryable()
                          join licensee in licensees
                          on brand.LicenseeId equals licensee.Id
                          where brand.Status == BrandStatus.Active
                          select brand).ToArray();
            if (status is Status.Active)
            {
                await _maintenanceRedisService.ClearResellerMaintenanceAsync(reseller.Id).ConfigureAwait(false);

                await _maintenanceRedisService.CleartLicenseeMaintenancesAsync(licensees.Select(x => x.Id).ToArray()).ConfigureAwait(false);

                await _maintenanceRedisService.CleartBrandMaintenancesAsync(brands.Select(x => x.Id).ToArray()).ConfigureAwait(false);
            }
            else
            {
                await _maintenanceRedisService.AddResellerMaintenanceAsync(reseller.Id).ConfigureAwait(false);

                await _maintenanceRedisService.AddLicenseeMaintenancesAsync(licensees.Select(x => x.Id).ToArray()).ConfigureAwait(false);

                await _maintenanceRedisService.AddBrandMaintenancesAsync(brands.Select(x => x.Id).ToArray()).ConfigureAwait(false);
            }
        }

    }

    Task<bool> IResellerService.ExistsAsync(Expression<Func<Reseller, bool>> predicate) =>
        _repository.Resellers.AnyAsync(predicate);

    IQueryable<Reseller> IResellerService.GetResellers(GetResellersRequest request, bool tracking)
    {
        var queryable = tracking ? _repository.Resellers : _repository.Resellers.AsNoTracking();
        return queryable.Where(x =>
            EF.Functions.ILike(x.Code, $"%{request.Keyword}%") ||
            EF.Functions.ILike(x.Name, $"%{request.Keyword}%") ||
            EF.Functions.ILike(x.Description, $"%{request.Keyword}%") ||
            EF.Functions.ILike(x.OperationalEmail, $"%{request.Keyword}%"));
    }

    private HashSet<ResellerLanguage> GenerateResellerLanguages(Reseller reseller, ResellerRequest request)
    {
        var resellerLanguages = _repository.Languages.AsQueryable()
            .Where(x => request.CultureCodes.Contains(x.CultureCode)).Select(x =>
                new ResellerLanguage
                {
                    ResellerId = reseller.Id,
                    CultureCode = x.CultureCode
                });
        return new HashSet<ResellerLanguage>(resellerLanguages);
    }

    private HashSet<ResellerCurrency> GenerateResellerCurrencies(Reseller reseller, ResellerRequest request)
    {
        var resellerCurrencies = _repository.Currencies.AsQueryable()
            .Where(x => request.CurrencyCodes.Contains(x.Code)).Select(x =>
                new ResellerCurrency
                {
                    ResellerId = reseller.Id,
                    CurrencyCode = x.Code
                });
        return new HashSet<ResellerCurrency>(resellerCurrencies);
    }

    private HashSet<ResellerGameProvider> GenerateResellerGameProviders(Reseller reseller, ResellerRequest request)
    {
        var resellerGameProviders = _repository.GameProviders.AsQueryable()
            .Where(x => request.GameProviderIds.Contains(x.Id)).Select(x =>
                new ResellerGameProvider
                {
                    ResellerId = reseller.Id,
                    GameProviderId = x.Id
                });
        return new HashSet<ResellerGameProvider>(resellerGameProviders);
    }

    private HashSet<ResellerBetLimit> GenerateResellerBetLimits(
        Reseller reseller, ResellerRequest request)
    {
        var betLimits = request.BetLimitIds.Select(id =>
            new ResellerBetLimit
            {
                ResellerId = reseller.Id,
                BetLimitId = id
            });
        return new HashSet<ResellerBetLimit>(betLimits);
    }
}