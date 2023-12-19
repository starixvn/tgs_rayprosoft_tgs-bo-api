using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.NotificationContacts;

public record NotificationContactEmailBrandCommand(
    Guid BrandId,
    IEnumerable<NotificationContactEmail> Emails);
