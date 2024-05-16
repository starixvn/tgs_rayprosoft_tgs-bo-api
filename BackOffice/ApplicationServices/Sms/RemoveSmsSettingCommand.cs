using System;

namespace TGS.Services.BackOffice.ApplicationServices.Sms;

public record RemoveSmsSettingCommand(Guid SmsSettingId);