using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.MessageQueue;
using Tgs.PlayerService.Client;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.BatchAdjust;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

public class BatchAdjustTests
{
    [Test]
    public async Task Check_Batch_Completed()
    {
        var fakeBatchTempRepository = Substitute.For<IBatchTempRepository>();

        var sut = new CheckBatchCompletedQueryService(fakeBatchTempRepository);

        var query = new CheckBatchCompletedQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = fakeBatchTempRepository.Received(1)
            .GetBatchIsCompletedAsync(Arg.Is(query.BatchId));
    }

    [Test]
    public async Task Export_Form_Template()
    {
        var sut = new ExportBatchAdjustFormTemplateCommandService();

        var command = new ExportBatchAdjustFormTemplateCommand();

        var actual = await sut.GetAsync(command);

        Assert.IsNotNull(actual);
    }

    [Test]
    public async Task Get_Batch_Result()
    {
        var fakeBatchTempRepository = Substitute.For<IBatchTempRepository>();

        var sut = new ProcessResultQueryService(fakeBatchTempRepository);

        var query = new ProcessResultQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeBatchTempRepository.Received(1)
            .GetAdjustBalanceItemResultsAsync(Arg.Is(query.BatchId));
    }

    [Test]
    public async Task Preview_Batch()
    {
        var fakeBatchTempRepository = Substitute.For<IBatchTempRepository>();

        var sut = new PreviewBatchQueryService(fakeBatchTempRepository);

        var query = new PreviewBatchQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeBatchTempRepository.Received(1)
            .GetAdjustBalanceBatchAsync(Arg.Is(query.BatchId));
    }

    [Test]
    public async Task Process_Batch_AdjustAmount()
    {
        var fakeBatchTempRepository = Substitute.For<IBatchTempRepository>();
        var fakePlayerAdjustBalanceService = Substitute.For<IPlayerAdjustBalanceService>();
        var fakeMessageSender = Substitute.For<IMessageSender>();

        var sut = new ProcessBatchCommandService(
            fakeBatchTempRepository,
            fakePlayerAdjustBalanceService,
            fakeMessageSender,
            NullLogger<ProcessBatchCommandService>.Instance);

        var command = new ProcessBatchCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            PlayerAdjustmentTransactionType.CompanyAdjust,
            true,
            Guid.NewGuid(),
            "modifier");

        _ = fakeBatchTempRepository.BidBatchAsync(Arg.Is(command.BatchId))
            .Returns(true);

        _ = fakeBatchTempRepository.GetAdjustBalanceBatchAsync(Arg.Is(command.BatchId))
            .Returns(
                new[] {
                    new BatchAdjustBalanceItem(
                        1,
                        command.BrandId,
                        Guid.NewGuid(),
                        "test1",
                        "test1",
                        100,
                        0,
                        null)
                }.ToAsyncEnumerable());

        await sut.ExecuteAsync(command);

        _ = fakePlayerAdjustBalanceService.Received(1)
            .ProcessAdjustApplyAsync(
                Arg.Any<PlayerBalanceAdjustmentApplyInfo>(),
                Arg.Is(command.ModifyUserId),
                Arg.Is(command.ModifyUserName));

        _ = fakeBatchTempRepository.Received(1)
            .SaveAdjustBalanceBatchResultAsync(Arg.Is(command.BatchId), Arg.Any<IEnumerable<BatchAdjustBalanceItemResult>>());

        _ = fakeBatchTempRepository.Received(1)
            .CompleteBatchAsync(Arg.Is(command.BatchId));

        _ = fakeMessageSender.Received(1)
            .SendAsync(
                Arg.Is("tgs.bo.notify"),
                Arg.Any<ReadOnlyMemory<byte>>());
    }

    [Test]
    public async Task Read_File_Data()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new ReadBatchAdjustItemsQueryService(fakePlayerServiceClient);

        var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        var fileInfo = fileProvider.GetFileInfo("/Assets/TestBatchAdjust.xlsx");

        await using var fileStream = fileInfo.CreateReadStream();

        var query = new ReadBatchAdjustItemsQuery(Guid.NewGuid(), fileStream);

        _ = fakePlayerServiceClient.GetPlayerInfoByUserNameAsync(Arg.Is(query.BrandId), Arg.Any<string>())
            .Returns(new PlayerInfo
            {
                PlayerCode = "test",
                BrandId = query.BrandId
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is("test"))
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = Guid.NewGuid().ToString()
            }.ToImmutableDictionary());

        var actual = await sut.QueryAsync(query).ToArrayAsync();

        Assert.AreEqual(50, actual.Length);

        var lastRow = actual.Last();

        Assert.AreEqual(50, lastRow.AdjustAmount);
        Assert.AreEqual(50, lastRow.Turnover);
    }

    [Test]
    public async Task Save_Batch()
    {
        var fakeBatchTempRepository = Substitute.For<IBatchTempRepository>();

        var sut = new SaveBatchCommandService(fakeBatchTempRepository);

        var command = new SaveBatchCommand(Array.Empty<BatchAdjustBalanceItem>().ToAsyncEnumerable());

        _ = await sut.GetAsync(command);

        _ = fakeBatchTempRepository.Received(1)
            .SaveAdjustBalanceBatchAsync(Arg.Any<IEnumerable<BatchAdjustBalanceItem>>());
    }
}
