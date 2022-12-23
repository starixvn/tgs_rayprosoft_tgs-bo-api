using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Apis.BackendOffice.Controllers.Messages.Licensee;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Infrastructure.RedisServices;

namespace TGS.Apis.BackendOffice.DataService.Services;

public interface ILicenseeService
{
    Task ChangeStatusAsync(Licensee licensee, Status status, string username);

    Task CreateLicenseeAsync(CreateLicenseeRequest request, string username);

    Task<bool> ExistsAsync(Expression<Func<Licensee, bool>> predicate);

    Task<Licensee> GetLicenseeAsync(
                    Expression<Func<Licensee, bool>> predicate,
        Func<IQueryable<Licensee>, IQueryable<Licensee>> preQuery = null,
        bool tracking = false);

    IQueryable<Licensee> GetLicensees(GetLicenseesRequest request, bool tracking = false);

    Task UpdateLicenseeAsync(Licensee licensee, UpdateLicenseeRequest request, string username);
}

public class LicenseeService : ILicenseeService
{
    private readonly IMaintenanceRedisService _maintenanceRedisService;
    private readonly ITgsRepository _repository;

    public LicenseeService(
        ITgsRepository tgsRepository,
        IMaintenanceRedisService maintenanceRedisService
        )
    {
        _repository = tgsRepository;
        _maintenanceRedisService = maintenanceRedisService ?? throw new ArgumentNullException(nameof(maintenanceRedisService));
    }

    async Task ILicenseeService.ChangeStatusAsync(Licensee licensee, Status status, string username)
    {
        licensee.Status = status;
        licensee.UpdatedOn = DateTimeOffset.Now;
        licensee.UpdatedByUsername = username;

        _repository.Entry(licensee).State = EntityState.Modified;

        _ = await _repository.SaveChangesAsync().ConfigureAwait(false);

        if (status is Status.Active or Status.Maintenance)
        {
            var reseller = _repository.Resellers.AsQueryable().Where(x => x.Id == licensee.ResellerId).First();
            var brands = _repository.Brands.AsQueryable().Where(x => x.LicenseeId == licensee.Id && x.Status == BrandStatus.Active);
            var isMaintenance = reseller.Status is Status.Active && status is Status.Active;
            if (isMaintenance)
            {
                await _maintenanceRedisService.CleartLicenseeMaintenanceAsync(licensee.Id).ConfigureAwait(false);
                await _maintenanceRedisService.CleartBrandMaintenancesAsync(brands.Select(x => x.Id).ToArray()).ConfigureAwait(false);
            }
            else
            {
                await _maintenanceRedisService.AddLicenseeMaintenanceAsync(licensee.Id).ConfigureAwait(false);
                await _maintenanceRedisService.AddBrandMaintenancesAsync(brands.Select(x => x.Id).ToArray()).ConfigureAwait(false);
            }
        }
    }

    async Task ILicenseeService.CreateLicenseeAsync(CreateLicenseeRequest request, string username)
    {
        var licensee = new Licensee
        {
            Id = Guid.NewGuid(),
            ResellerId = request.ResellerId,
            Code = request.Code,
            Name = request.Name,
            Status = Status.Active,
            Description = request.Description,
            OperationalEmail = request.OperationalEmail,
            CreatedByUsername = username,
            UpdatedByUsername = username,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        licensee.Languages = GenerateLicenseeLanguages(licensee, request);
        licensee.Currencies = GenerateLicenseeCurrencies(licensee, request);
        licensee.GameProviders = GenerateLicenseeGameProviders(licensee, request);
        licensee.BetLimits = GenerateLicenseeBetLimits(licensee, request);

        _ = await _repository.Licensees.AddAsync(licensee).ConfigureAwait(false);
        _ = await _repository.SaveChangesAsync().ConfigureAwait(false);
    }

    Task<bool> ILicenseeService.ExistsAsync(Expression<Func<Licensee, bool>> predicate) =>
        _repository.Licensees.AnyAsync(predicate);

    Task<Licensee> ILicenseeService.GetLicenseeAsync(Expression<Func<Licensee, bool>> predicate,
            Func<IQueryable<Licensee>, IQueryable<Licensee>> preQuery, bool tracking)
    {
        var licensee = tracking ? _repository.Licensees : _repository.Licensees.AsNoTracking();
        preQuery ??= c => c;

        return preQuery(licensee).FirstOrDefaultAsync(predicate);
    }

    IQueryable<Licensee> ILicenseeService.GetLicensees(GetLicenseesRequest request, bool tracking)
    {
        var queryable = tracking ? _repository.Licensees : _repository.Licensees.AsNoTracking();
        return queryable.Where(x =>
            EF.Functions.ILike(x.Code, $"%{request.Keyword}%") ||
            EF.Functions.ILike(x.Name, $"%{request.Keyword}%") ||
            EF.Functions.ILike(x.Description, $"%{request.Keyword}%") ||
            EF.Functions.ILike(x.OperationalEmail, $"%{request.Keyword}%"));
    }

    Task ILicenseeService.UpdateLicenseeAsync(Licensee licensee, UpdateLicenseeRequest request, string username)
    {
        _repository.LicenseeLanguages.RemoveRange(licensee.Languages);
        _repository.LicenseeCurrencies.RemoveRange(licensee.Currencies);
        _repository.LicenseeGameProviders.RemoveRange(licensee.GameProviders);
        _repository.LicenseeBetLimits.RemoveRange(licensee.BetLimits);

        licensee.Code = request.Code;
        licensee.Name = request.Name;
        licensee.Description = request.Description;
        licensee.OperationalEmail = request.OperationalEmail;
        licensee.UpdatedByUsername = username;
        licensee.UpdatedOn = DateTimeOffset.Now;
        licensee.Languages = GenerateLicenseeLanguages(licensee, request);
        licensee.Currencies = GenerateLicenseeCurrencies(licensee, request);
        licensee.GameProviders = GenerateLicenseeGameProviders(licensee, request);
        licensee.BetLimits = GenerateLicenseeBetLimits(licensee, request);

        _repository.Entry(licensee).State = EntityState.Modified;

        return _repository.SaveChangesAsync();
    }

    private HashSet<LicenseeBetLimit> GenerateLicenseeBetLimits(Licensee licensee, LicenseeRequest request)
    {
        var licenseeBetLimits = _repository.BetLimits.AsQueryable()
            .Where(x => request.BetLimitIds.Contains(x.Id)).Select(x =>
                new LicenseeBetLimit
                {
                    LicenseeId = licensee.Id,
                    BetLimitId = x.Id
                });
        return new HashSet<LicenseeBetLimit>(licenseeBetLimits);
    }

    private HashSet<LicenseeCurrency> GenerateLicenseeCurrencies(Licensee licensee, LicenseeRequest request)
    {
        var licenseeCurrencies = _repository.Currencies.AsQueryable()
            .Where(x => request.CurrencyCodes.Contains(x.Code)).Select(x =>
                new LicenseeCurrency
                {
                    LicenseeId = licensee.Id,
                    CurrencyCode = x.Code
                });
        return new HashSet<LicenseeCurrency>(licenseeCurrencies);
    }

    private HashSet<LicenseeGameProvider> GenerateLicenseeGameProviders(Licensee licensee, LicenseeRequest request)
    {
        var licenseeGameProviders = _repository.GameProviders.AsQueryable()
            .Where(x => request.GameProviderIds.Contains(x.Id)).Select(x =>
                new LicenseeGameProvider
                {
                    LicenseeId = licensee.Id,
                    GameProviderId = x.Id
                });
        return new HashSet<LicenseeGameProvider>(licenseeGameProviders);
    }

    private HashSet<LicenseeLanguage> GenerateLicenseeLanguages(Licensee licensee, LicenseeRequest request)
    {
        var licenseeLanguages = _repository.Languages.AsQueryable()
            .Where(x => request.CultureCodes.Contains(x.CultureCode)).Select(x =>
                new LicenseeLanguage
                {
                    LicenseeId = licensee.Id,
                    CultureCode = x.CultureCode
                });
        return new HashSet<LicenseeLanguage>(licenseeLanguages);
    }
}
