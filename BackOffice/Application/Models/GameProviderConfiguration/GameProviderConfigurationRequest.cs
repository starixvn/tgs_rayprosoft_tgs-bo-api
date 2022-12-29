using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;

public class GameProviderConfigurationRequest : BaseRequest
{
    public string ClientId { get; set; }

    public string ConfigurationData { get; set; } = "";

    public string CreatedByUsername { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string Description { get; set; }

    public string DesktopEndpoint { get; set; } = "";

    public Guid GameProviderId { get; set; }

    public Guid Id { get; set; }

    public bool IsDefault { get; set; }

    public string MobileEndpoint { get; set; } = "";

    public string Name { get; set; }

    public string SecretKey { get; set; }

    public Status Status { get; set; }

    public string UpdatedByUsername { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }
}
