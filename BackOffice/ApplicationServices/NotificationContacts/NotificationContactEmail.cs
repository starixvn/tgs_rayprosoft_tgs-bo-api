using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.NotificationContacts;

public record NotificationContactEmail(
    string Name,
    string Email,
    string Remark);
