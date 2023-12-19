using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Tgs.BrandCertificate.Clients;
using Tgs.Notification.Contacts.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.NotificationContacts;

internal class NotificationContactEmailBrandQueryService(
    IBrandServiceClient brandServiceClient,
    INotificationContactsClient client) : IQueryService<NotificationContactEmailBrandQuery, NotificationContactEmail>
{
    public async IAsyncEnumerable<NotificationContactEmail> QueryAsync(NotificationContactEmailBrandQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var brandInfo = await brandServiceClient.GetBrandInfoAsync(
            brandId: query.BrandId,
            cancellationToken: cancellationToken)
            ?? throw new NullReferenceException($"This brand does not exist.{query.BrandId}");

        var emailContact = await client.EmailBrandGetAsync(
            code: brandInfo.Code,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (emailContact is null)
        {
            yield break;
        }

        if (!emailContact.Contacts.Any())
        {
            yield break;
        }

        foreach (var contact in emailContact.Contacts)
        {
            yield return new NotificationContactEmail(
                contact.Name,
                contact.Email,
                contact.Remark);
        }
    }
}
