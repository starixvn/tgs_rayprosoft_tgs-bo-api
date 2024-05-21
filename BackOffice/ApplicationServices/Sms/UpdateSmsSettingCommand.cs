using System;
using System.Text.Json;

namespace TGS.Services.BackOffice.ApplicationServices.Sms;

public record UpdateSmsSettingCommand(
    Guid SmsSettingId,
    string Name,
    string Code,
    JsonDocument Setting,
    string Modifier);