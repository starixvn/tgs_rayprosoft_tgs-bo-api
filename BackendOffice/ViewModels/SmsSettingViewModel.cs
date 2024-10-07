using System;
using System.Text.Json;

namespace TGS.Apis.BackendOffice.ViewModels;

public class SmsSettingViewModel
{
    public Guid SmsSettingId { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public JsonDocument Setting { get; set; }
}