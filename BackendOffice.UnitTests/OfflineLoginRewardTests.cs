using Microsoft.Extensions.FileProviders;
using NSubstitute;
using System.Reflection;
using TGS.Services.BackOffice.ApplicationServices.Promotion;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Tests.BackendOffice.UnitTests;

internal class OfflineLoginRewardTests
{
    [Test]
    public async Task 下載Excel範本()
    {
        var sut = new ExportOfflineLoginRewardFormTemplateCommandService();

        var command = new ExportOfflineLoginRewardFormTemplateCommand();

        var actual = await sut.GetAsync(command);

        Assert.IsNotNull(actual);
    }

    [Test]
    public async Task 讀取Excel_UserList資料()
    {
        var sut = new ReadOfflineLoginRewardUserListQueryService();
        
        var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        var fileInfo = fileProvider.GetFileInfo("/Assets/TestOfflineLoginReward.xlsx");

        await using var fileStream = fileInfo.CreateReadStream();

        var query = new ReadOfflineLoginRewardUserListQuery(fileStream);

        var actual = await sut.QueryAsync(query).ToArrayAsync();

        Assert.AreEqual(5, actual.Length);
    }

    [Test]
    public async Task 儲存上傳的Temp_User_Batch資料()
    {
        var fakeBatchTempRepository = Substitute.For<IBatchTempRepository>();

        var sut = new SaveOfflineLoginRewardTempUserBatchCommandService(fakeBatchTempRepository);

        var batchId = Guid.NewGuid();

        var batchData = new OfflineLoginRewardTempUserBatch(
            Guid.NewGuid(),
            new[] { "user_name_1", "user_name_2" });
        
        var command = new SaveOfflineLoginRewardTempUserBatchCommand(
            batchData.BrandId,
            batchData.UserNames);

        _ = await sut.GetAsync(command);

        _ = fakeBatchTempRepository
            .Received(1)
            .SaveOfflineLoginRewardTempUserBatchAsync(
                Arg.Is(batchData),
                Arg.Any<CancellationToken>());
    }
}