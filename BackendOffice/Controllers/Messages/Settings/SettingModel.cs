using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Settings;

public class SettingModel
{
    public string CreatedByUsername { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string Description { get; set; }

    public string Key { get; set; }

    public string UpdatedByUsername { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }

    public string Value { get; set; }
}
