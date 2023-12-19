using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.Notification.Contacts.Client;
using Tgs.Notification.Contacts.Common.Models;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.NotificationContacts;

internal class NotificationContactEmailBrandCommandService(
    IBrandServiceClient brandServiceClient,
    INotificationContactsClient client) : ICommandService<NotificationContactEmailBrandCommand>
{
    public async ValueTask ExecuteAsync(NotificationContactEmailBrandCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.Emails.Any())
        {
            return;
        }

        var brandInfo = await brandServiceClient.GetBrandInfoAsync(
            brandId: command.BrandId,
            cancellationToken: cancellationToken)
            ?? throw new NullReferenceException($"This brand does not exist.{command.BrandId}");

        var emails = command.Emails.Select(selector: selector => new EmailContact
        {
            Name = selector.Name,
            Email = selector.Email,
            Remark = selector.Remark,
        }).ToArray();

        await client.EmailBrandMergeAsync(
            code: brandInfo.Code,
            contacts: emails,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
