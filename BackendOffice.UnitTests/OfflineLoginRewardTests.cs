using Microsoft.Extensions.FileProviders;
using System.Reflection;
using TGS.Services.BackOffice.ApplicationServices.Promotion;

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
}