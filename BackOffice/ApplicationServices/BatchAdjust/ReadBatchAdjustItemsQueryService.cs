using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

#nullable enable

namespace TGS.Services.BackOffice.ApplicationServices.BatchAdjust;

internal class ReadBatchAdjustItemsQueryService : IQueryService<ReadBatchAdjustItemsQuery, BatchAdjustBalanceItem>
{
    private readonly IPlayerServiceClient _playerServiceClient;

    public ReadBatchAdjustItemsQueryService(IPlayerServiceClient playerServiceClient)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    }

    public async IAsyncEnumerable<BatchAdjustBalanceItem> QueryAsync(
        ReadBatchAdjustItemsQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new XLWorkbook(query.ImportFile);

        var sheet = reader.Worksheet(1);

        var index = 2;

        while (index < 52)
        {
            var userName = sheet.Cell(index, 1).GetFormattedString();

            if (string.IsNullOrWhiteSpace(userName))
                break;

            var playerInfo = await _playerServiceClient.GetPlayerInfoByUserNameAsync(
                query.BrandId,
                userName,
                cancellationToken).ConfigureAwait(false);

            if (playerInfo is null)
                break;

            var playerProps = await _playerServiceClient
                .GetPlayerPropertiesAsync(playerInfo.PlayerCode, cancellationToken)
                .ConfigureAwait(false);

            var walletId = Guid.Parse(playerProps["DefaultWalletId"]);

            var adjustAmount = sheet.Cell(index, 2).GetValue<decimal>();
            var turnover = sheet.Cell(index, 3).GetValue<decimal>();
            var remark = sheet.Cell(index, 4).GetFormattedString();

            var item = new BatchAdjustBalanceItem(
                index - 2,
                playerInfo.BrandId,
                walletId,
                playerInfo.PlayerCode,
                playerInfo.UserName,
                adjustAmount,
                turnover < 0M ? 0M : turnover,
                remark);

            yield return item;

            index++;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
