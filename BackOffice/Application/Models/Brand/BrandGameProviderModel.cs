using System;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class BrandGameProviderModel
{
    public Guid ConfigurationId { get; set; }

    public string DefaultLanguageCode { get; set; }

    public string DesktopQueryString { get; set; }

    public Guid GameProviderId { get; set; }

    public Guid? Id { get; set; }

    public string MobileQueryString { get; set; }

    public string Name { get; set; }
}
