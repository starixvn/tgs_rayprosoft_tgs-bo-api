using System;
using System.Threading.Tasks;

namespace TGS.Services.BackOffice.Domain;

public interface IMerchantRepository
{
    Task<bool> ExistBrandInLicenseesAsync(Guid brandId, Guid licenseeId);

    Task<bool> ExistBrandInResellersAsync(Guid brandId, Guid resellerId);

    Task<bool> ExistLicenseeInResellersAsync(Guid licenseeId, Guid resellerId);
}