using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.ViewModels;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.NotificationContacts;
using System.Collections.Generic;
using System.Linq;

namespace TGS.Apis.BackendOffice.Controllers.NotificationContacts;

/// <summary>
/// Notification Contact Brand 
/// </summary>
/// <seealso cref="TGS.Apis.BackendOffice.Controllers.BaseContextController" />
[Route("api/[controller]")]
[ApiController]
public class NoteContBrand : BaseContextController
{
    [HttpGet("{brandId:guid}/email", Name = "NotificationContactEmailBrand_Query")]
    public async IAsyncEnumerable<NotificationContactEmailViewModel> QueryAsync(
    [FromServices] IQueryService<NotificationContactEmailBrandQuery, NotificationContactEmail> service,
    Guid brandId)
    {
        var emails = await service.QueryAsync(
            query: new NotificationContactEmailBrandQuery(brandId),
            cancellationToken: HttpContext.RequestAborted)
            .ToArrayAsync(
            cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false)
            ?? Enumerable.Empty<NotificationContactEmail>();

        if (!emails.Any())
        {
            yield break;
        }

        foreach (var email in emails)
        {
            yield return new NotificationContactEmailViewModel
            {
                Name = email.Name,
                Email = email.Email,
                Remark = email.Remark
            };
        }
    }
    [HttpPost("{brandId:guid}/email", Name = "NotificationContactEmailBrand_Command")]
    public ValueTask ExecuteAsync(
        [FromServices] ICommandService<NotificationContactEmailBrandCommand> service,
        Guid brandId,
        [FromBody] IEnumerable<NotificationContactEmailViewModel> emails)
    {
        if (!emails.Any())
        {
            throw new NullReferenceException(nameof(emails));
        }

        var command = new NotificationContactEmailBrandCommand(
            brandId,
            emails.Select(selector: selector => new NotificationContactEmail(
                selector.Name,
                selector.Email,
                selector.Remark))

            );
        return service.ExecuteAsync(
            command: command,
            cancellationToken: HttpContext.RequestAborted);
    }
}
