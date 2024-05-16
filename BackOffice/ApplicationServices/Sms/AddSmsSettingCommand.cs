using System.Text.Json;

namespace TGS.Services.BackOffice.ApplicationServices.Sms;

public record AddSmsSettingCommand(
    string Name,
    string Code,
    JsonDocument Setting,
    string Modifier);
