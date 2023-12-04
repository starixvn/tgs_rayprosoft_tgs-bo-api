using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

internal class CreateIdentityAuthenticationFormCommandService(
    IBrandService brandService,
    IPlayerService playerService) : IGetService<CreateIdentityAuthenticationFormCommand, MessageResponse>
{
    private readonly IBrandService _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
    private readonly IPlayerService _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));

    public async ValueTask<MessageResponse> GetAsync(CreateIdentityAuthenticationFormCommand query, CancellationToken cancellationToken = default)
    {
        var brand = await _brandService
            .GetBrandAsync(b => b.Id == query.BrandId)
            .ConfigureAwait(false);

        if (!brand.EnablePlayerIdentity)
            return new MessageResponse()
            {
                Valid = false,
                Message = $"Brand({brand.Name}) is disabled PlayerIdentity"
            };

        var data = JsonSerializer.Serialize(new PlayerAuthenticationFormData(query.FileId));

        var formId = await _playerService
            .CreatePlayerAuthenticationFormAsync(query.BrandId, query.PlayerCode, data)
            .ConfigureAwait(false);

        await _playerService
            .UpdatePlayerIdentityAsync(query.PlayerCode, formId)
            .ConfigureAwait(false);

        return new MessageResponse()
        {
            Valid = true
        };
    }
}
