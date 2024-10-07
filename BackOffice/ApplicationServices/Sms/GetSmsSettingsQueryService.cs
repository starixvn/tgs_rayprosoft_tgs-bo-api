using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.SmsService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.Sms;

internal class GetSmsSettingsQueryService(ISmsServiceClient client) : IQueryService<GetSmsSettingsQuery, SmsSettingInfo>
{
    public IAsyncEnumerable<SmsSettingInfo> QueryAsync(GetSmsSettingsQuery query, CancellationToken cancellationToken = default)
        => client.QuerySmsSettingListAsync(cancellationToken);
}