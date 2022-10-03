using System;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class BrandGameProviderModel
{
    public Guid? Id { get; set; }
    public Guid GameProviderId { get; set; }
    public string Name { get; set; }
    public Guid ConfigurationId { get; set; }
    public string DefaultLanguageCode { get; set; }
    public string DesktopQueryString { get; set; }
    public string MobileQueryString { get; set; }
}