using System;

namespace TGS.Services.BackOffice.ApplicationServices;
public record ListPlayerCodesByUserNamePrefixQuery(
    Guid BrandId,
    string UserNamePrefix);
