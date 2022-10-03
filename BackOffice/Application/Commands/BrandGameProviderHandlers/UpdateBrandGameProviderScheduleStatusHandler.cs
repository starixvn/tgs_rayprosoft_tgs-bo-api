using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandGameProviderHandlers;

public class
    UpdateBrandGameProviderScheduleStatusHandler : Handler<UpdateBrandGameProviderStatusRequest, MessageResponse>
{
    private readonly IRedisCacheManager _cacheManager;
    private readonly IBrandGameProviderService _service;


    public UpdateBrandGameProviderScheduleStatusHandler(IBrandGameProviderService service,
        IRedisCacheManager cacheManager,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
        _cacheManager = cacheManager;
    }

    public override async Task<MessageResponse> Handle(UpdateBrandGameProviderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var brandGameProvider = await _service.GetBrandGameProviderAsync(q => q.Id == request.Id);

        if (brandGameProvider == null)
        {
            return CreateInvalidResponse<MessageResponse>(BrandGameProvider.NotFound);
        }

        brandGameProvider.Status = request.Status;
        brandGameProvider.IsManualChangeStatus = request.Status == Status.Maintenance;

        await _service.UpdateBrandGameProviderAsync(brandGameProvider);
        await _cacheManager.ClearCacheAsync(RedisCacheType.BrandGameProviders, brandGameProvider.BrandId);

        return CreateValidResponse<MessageResponse>();
    }
}